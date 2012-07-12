﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MySoft.Mail;
using System.Configuration;

namespace MySoft.Logger
{
    /// <summary>
    /// 简单日志管理类(按日期生成文件)
    /// </summary>
    public class SimpleLog
    {
        /// <summary>
        /// 简单日志的单例 (默认路径为根目录下的Logs目录)
        /// </summary>
        public static readonly SimpleLog Instance;
        private static readonly Queue<LogInfo> logqueue;

        static SimpleLog()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var tmpdir = ConfigurationManager.AppSettings["SimpleLogDir"];
            if (!string.IsNullOrEmpty(tmpdir)) dir = tmpdir;

            Instance = new SimpleLog(dir);
            logqueue = new Queue<LogInfo>();

            //启动生成文件线程
            ThreadPool.QueueUserWorkItem(state =>
            {
                while (true)
                {
                    //判断日志数
                    if (logqueue.Count == 0)
                    {
                        Thread.Sleep(100);

                        continue;
                    }
                    else
                    {
                        LogInfo item = null;

                        lock (logqueue)
                        {
                            item = logqueue.Dequeue();
                        }

                        if (item != null)
                        {
                            try
                            {
                                string dirPath = Path.GetDirectoryName(item.FilePath);
                                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                                File.AppendAllText(item.FilePath, item.Log, Encoding.UTF8);
                            }
                            catch (IOException)
                            {
                                //写日志出错重回队列
                                lock (logqueue)
                                {
                                    logqueue.Enqueue(item);
                                }
                            }
                            catch
                            {
                                //其它错误不做处理
                            }
                        }

                        //等待10毫秒
                        Thread.Sleep(10);
                    }
                }
            });
        }

        private string basedir;

        /// <summary>
        /// 设置基准路径
        /// </summary>
        /// <param name="basedir"></param>
        public void SetBaseDir(string basedir)
        {
            this.basedir = basedir;
        }

        /// <summary>
        /// 实例化简单日志组件
        /// </summary>
        /// <param name="basedir">日志存储根目录，下面会自动创建Log与ErrorLog文件夹</param>
        public SimpleLog(string basedir)
        {
            this.basedir = basedir;
        }

        #region 自动创建文件

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="ex"></param>
        public void WriteLog(Exception ex)
        {
            WriteLogForDir(string.Empty, ex);
        }

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="ex"></param>
        public void WriteLogForDir(string dir, Exception ex)
        {
            if (ex == null) return;

            string filePath = Path.Combine(Path.Combine(basedir, "ErrorLogs"), dir);
            filePath = Path.Combine(filePath, DateTime.Now.ToString("yyyy-MM-dd"));

            var appName = GetApplicationName(ex);
            if (!string.IsNullOrEmpty(appName)) filePath = Path.Combine(filePath, appName);

            filePath = Path.Combine(filePath, string.Format("{0}.log", GetServiceFile(ex)));

            WriteFileLog(filePath, ex, false);
        }

        /// <summary>
        /// 获取服务路径
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetServiceFile(Exception ex)
        {
            var errorName = ex.GetType().Name;

            //获取内部异常
            if (ex.InnerException != null)
                errorName = ex.InnerException.GetType().Name;

            return errorName;
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        public void WriteLog(string log)
        {
            WriteLogForDir(string.Empty, log);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        public void WriteLogForDir(string dir, string log)
        {
            if (string.IsNullOrEmpty(log)) return;

            string filePath = Path.Combine(Path.Combine(basedir, "Logs"), dir);
            filePath = Path.Combine(filePath, string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd")));

            WriteFileLog(filePath, log, false);
        }

        /// <summary>
        /// 写日志并发送邮件
        /// </summary>
        /// <param name="log"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string log, string mailTo)
        {
            WriteLogWithSendMail(log, new string[] { mailTo });
        }

        /// <summary>
        /// 写日志并发送邮件
        /// </summary>
        /// <param name="log"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string log, string[] mailTo)
        {
            WriteLog(log);
            SendMail(log, mailTo);
        }

        /// <summary>
        /// 写错误日志并发送邮件
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(Exception ex, string mailTo)
        {
            WriteLogWithSendMail(ex, new string[] { mailTo });
        }

        /// <summary>
        /// 写错误日志并发送邮件
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(Exception ex, string[] mailTo)
        {
            WriteLog(ex);
            SendMail(ex, mailTo);
        }

        #endregion

        #region 传入文件信息

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ex"></param>
        public void WriteLogForFile(string fileName, Exception ex)
        {
            string log = ErrorHelper.GetErrorWithoutHtml(ex);
            WriteLogForFile(fileName, log);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="log"></param>
        public void WriteLogForFile(string fileName, string log)
        {
            string filePath = Path.Combine(basedir, fileName);
            WriteFileLog(filePath, log, false);
        }

        /// <summary>
        /// 写日志并发送邮件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="log"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string fileName, string log, string mailTo)
        {
            WriteLogWithSendMail(fileName, log, new string[] { mailTo });
        }

        /// <summary>
        /// 写日志并发送邮件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="log"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string fileName, string log, string[] mailTo)
        {
            WriteLogForFile(fileName, log);
            SendMail(log, mailTo);
        }

        /// <summary>
        /// 写错误日志并发送邮件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ex"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string fileName, Exception ex, string mailTo)
        {
            WriteLogWithSendMail(fileName, ex, new string[] { mailTo });
        }

        /// <summary>
        /// 写错误日志并发送邮件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ex"></param>
        /// <param name="mailTo"></param>
        public void WriteLogWithSendMail(string fileName, Exception ex, string[] mailTo)
        {
            WriteLogForFile(fileName, ex);
            SendMail(ex, mailTo);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="log"></param>
        /// <param name="to"></param>
        private void SendMail(string log, string[] to)
        {
            if (to == null || to.Length == 0)
            {
                throw new ArgumentException("请传入收件人地址信息参数！");
            }

            var body = CoreHelper.GetSubString(log, 20, "...");
            string title = string.Format("{2} - 普通邮件由【{0}({1})】发出", DnsHelper.GetHostName(), DnsHelper.GetIPAddress(), body);
            SmtpMail.Instance.SendAsync(title, log, to);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="to"></param>
        private void SendMail(Exception ex, string[] to)
        {
            if (to == null || to.Length == 0)
            {
                throw new ArgumentException("请传入收件人地址信息参数！");
            }

            string title = string.Format("【{2}】({3}) - 异常邮件由【{0}({1})】发出", DnsHelper.GetHostName(), DnsHelper.GetIPAddress(),
                                        GetAppTitle(ex), GetServiceTitle(ex));

            SmtpMail.Instance.SendExceptionAsync(ex, title, to);
        }

        /// <summary>
        /// 获取App标题
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetAppTitle(Exception ex)
        {
            var appName = GetApplicationName(ex);
            return string.IsNullOrEmpty(appName) ? "未知应用" : appName;
        }

        /// <summary>
        /// 获取服务标题
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string GetServiceTitle(Exception ex)
        {
            var serviceName = GetServiceName(ex);
            var errorName = ex.GetType().Name;

            //获取内部异常
            if (ex.InnerException != null)
                errorName = ex.InnerException.GetType().Name;

            return string.IsNullOrEmpty(serviceName) ? errorName : errorName + " : " + serviceName;
        }

        private string GetApplicationName(Exception ex)
        {
            if (ex == null) return null;
            if (ex.Data.Contains("ApplicationName"))
                return ex.Data["ApplicationName"].ToString();
            else
                return null;
        }

        private string GetServiceName(Exception ex)
        {
            if (ex == null) return null;
            if (ex.Data.Contains("ServiceName"))
                return ex.Data["ServiceName"].ToString();
            else
                return null;
        }

        /// <summary>
        /// 写文件日志静态方法（传入文件绝对路径与文件内容）
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        public static void WriteFile(string filePath, string text)
        {
            WriteFile(filePath, text, false);
        }

        /// <summary>
        /// 写文件日志静态方法（传入文件绝对路径与文件内容）
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        /// <param name="delFile"></param>
        public static void WriteFile(string filePath, string text, bool delFile)
        {
            if (delFile && File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                }
            }

            WriteFileLog(filePath, text, true);
        }

        /// <summary>
        /// 写文件日志
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ex"></param>
        private static void WriteFileLog(string filePath, Exception ex, bool isOriginal)
        {
            string log = ErrorHelper.GetErrorWithoutHtml(ex);
            WriteFileLog(filePath, log, isOriginal);
        }

        /// <summary>
        /// 写文件日志
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="log"></param>
        private static void WriteFileLog(string filePath, string log, bool isOriginal)
        {
            lock (logqueue)
            {
                if (!isOriginal)
                {
                    log = string.Format("【{0}】 => {1}{2}{2}{3}{2}{2}", DateTime.Now, log,
                                        Environment.NewLine, string.Empty.PadRight(180, '='));
                }

                var loginfo = new LogInfo { FilePath = filePath, Log = log };

                //将信息入队列
                logqueue.Enqueue(loginfo);
            }
        }

        #endregion

        /// <summary>
        /// 日志信息
        /// </summary>
        private class LogInfo
        {
            /// <summary>
            /// 文件路径
            /// </summary>
            public string FilePath { get; set; }

            /// <summary>
            /// 日志内容
            /// </summary>
            public string Log { get; set; }
        }
    }
}
