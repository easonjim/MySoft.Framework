﻿using System;
using System.Threading;
using MySoft.IoC.Communication.Scs.Communication;
using MySoft.IoC.Communication.Scs.Communication.Messages;
using MySoft.IoC.Communication.Scs.Server;
using MySoft.IoC.Configuration;
using MySoft.IoC.Messages;
using MySoft.IoC.Services;
using MySoft.Logger;

namespace MySoft.IoC
{
    /// <summary>
    /// 服务通道
    /// </summary>
    internal class ServiceChannel : IDisposable
    {
        public event EventHandler<CallEventArgs> Callback;

        private ILog logger;
        private ServiceCaller caller;
        private ServerStatusService status;
        private int timeout;
        private Semaphore semaphore;

        /// <summary>
        /// 实例化ServiceChannel
        /// </summary>
        /// <param name="config"></param>
        /// <param name="caller"></param>
        /// <param name="status"></param>
        /// <param name="logger"></param>
        public ServiceChannel(CastleServiceConfiguration config, ServiceCaller caller, ServerStatusService status, ILog logger)
        {
            this.caller = caller;
            this.status = status;
            this.logger = logger;
            this.timeout = config.Timeout;
            this.semaphore = new Semaphore(config.MaxCaller, config.MaxCaller);
        }

        /// <summary>
        /// 发送响应消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        public void SendResponse(IScsServerClient channel, CallerContext e)
        {
#if DEBUG
            var message = string.Format("{0}：{1}({2})", e.Caller.AppName, e.Caller.HostName, e.Caller.IPAddress);
            var body = string.Format("Remote client【{0}】begin call service ({1},{2}).\r\nParameters => {3}",
                                        message, e.Caller.ServiceName, e.Caller.MethodName, e.Caller.Parameters);

            logger.WriteLog(body, LogType.Normal);
#endif

            //请求一个控制器
            semaphore.WaitOne(Timeout.Infinite, false);

            try
            {
                //响应请求
                HandleResponse(channel, e);
            }
            finally
            {
                //释放一个控制器
                semaphore.Release();
            }
        }

        /// <summary>
        /// 处理响应
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        private void HandleResponse(IScsServerClient channel, CallerContext e)
        {
            using (var channelResult = new ChannelResult(channel, e))
            {
                //开始异步调用
                ThreadPool.QueueUserWorkItem(WaitCallback, channelResult);

                //等待超时响应
                if (!channelResult.WaitOne(TimeSpan.FromSeconds(timeout)))
                {
                    //获取异常响应
                    e.Message = GetTimeoutResponse(e.Request, "Work item timeout.");
                }
                else
                {
                    //正常响应信息
                    e.Message = channelResult.Message;

                    //不是从缓存读取，则响应与状态服务跳过
                    if (e.Message == null)
                    {
                        //创建一个响应信息
                        e.Message = IoCHelper.GetResponse(e.Request, null);
                    }
                }
            }

            //处理响应信息
            HandleResponse(e);

            //发送消息
            SendMessage(channel, e);
        }

        /// <summary>
        /// 响应信息
        /// </summary>
        /// <param name="state"></param>
        private void WaitCallback(object state)
        {
            var channelResult = state as ChannelResult;

            try
            {
                //调用响应信息
                var channel = channelResult.Channel;
                var context = channelResult.Context;

                if (channel != null && channel.CommunicationState == CommunicationStates.Connected)
                {
                    //返回响应
                    var resMsg = caller.InvokeResponse(channel, context);

                    channelResult.Set(resMsg);
                }
                else
                {
                    channelResult.Set(null);
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 获取超时响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private ResponseMessage GetTimeoutResponse(RequestMessage reqMsg, string message)
        {
            //获取异常响应信息
            var body = string.Format("Async call service ({0}, {1})  timeout ({2}) ms. {3}",
                        reqMsg.ServiceName, reqMsg.MethodName, timeout * 1000, message);

            var resMsg = IoCHelper.GetResponse(reqMsg, new TimeoutException(body));

            //设置耗时时间
            resMsg.ElapsedTime = timeout * 1000;

            return resMsg;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="e"></param>
        private void HandleResponse(CallerContext e)
        {
            if (e.Message == null) return;

            try
            {
                //状态服务不统计
                if (e.Caller.ServiceName != typeof(IStatusService).FullName)
                {
                    //调用参数
                    var callArgs = new CallEventArgs(e.Caller)
                    {
                        ElapsedTime = e.Message.ElapsedTime,
                        Count = Math.Max(e.Message.Count, e.Count),
                        Error = e.Message.Error,
                        Value = e.Message.Value
                    };

                    //调用计数服务
                    status.Counter(callArgs);

                    //开始调用
                    if (Callback != null) Callback(this, callArgs);
                }
            }
            catch (Exception ex) { }
            finally
            {
                //设置消息异常
                SetMessageError(e);
            }
        }

        /// <summary>
        /// 设置消息异常
        /// </summary>
        /// <param name="e"></param>
        private void SetMessageError(CallerContext e)
        {
            if (e.Message == null) return;

            //如果是Json方式调用，则需要处理异常
            if (e.Request.InvokeMethod && e.Message.IsError)
            {
                //获取最底层异常信息
                var error = ErrorHelper.GetInnerException(e.Message.Error);

                e.Message.Error = new ApplicationException(error.Message);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="e"></param>
        private void SendMessage(IScsServerClient channel, CallerContext e)
        {
            if (channel.CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            try
            {
                //如果没有返回消息，则退出
                if (e.Buffer == null && e.Message == null)
                {
                    return;
                }

                IScsMessage message = null;

                if (e.Buffer != null)
                    message = new ScsRawDataMessage(e.Buffer, e.MessageId);
                else
                    message = new ScsResultMessage(e.Message, e.MessageId);

                //发送消息
                channel.SendMessage(message);
            }
            catch (Exception ex)
            {
                //获取异常响应
                var title = string.Format("Sending message ({0}, {1}) error.", e.Caller.ServiceName, e.Caller.MethodName);
                var error = IoCHelper.GetException(e.Caller, title, ex);

                throw error;
            }
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.semaphore.Close();

            this.caller = null;
            this.status = null;
        }

        #endregion
    }
}
