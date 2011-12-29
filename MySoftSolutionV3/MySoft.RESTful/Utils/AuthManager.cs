﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ServiceModel.Web;
using System.Web;
using MySoft.Logger;
using MySoft.RESTful.Auth;
using Castle.Core.Resource;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor;
using System.Configuration;
using Castle.Core;

namespace MySoft.RESTful.Utils
{
    /// <summary>
    /// 认证工厂
    /// </summary>
    public static class AuthManager
    {
        /// <summary>
        /// RESTful配置文件
        /// </summary>
        private static IList<IAuthentication> auths = new List<IAuthentication>();

        static AuthManager()
        {
            //读取配置文件
            var container = new WindsorContainer();
            if (ConfigurationManager.GetSection("mysoft.framework/auth") != null)
                container = new WindsorContainer(new XmlInterpreter(new ConfigResource("mysoft.framework/auth")));

            foreach (var node in container.Kernel.GraphNodes)
            {
                try
                {
                    var serviceType = (node as ComponentModel).Service;
                    var instance = container.Resolve(serviceType);
                    if (instance != null && instance is IAuthentication)
                    {
                        auths.Add((IAuthentication)instance);
                    }
                }
                catch (Exception ex)
                {
                    SimpleLog.Instance.WriteLog(ex);
                }
            }
        }

        /// <summary>
        /// 初始化上下文
        /// </summary>
        private static void InitializeContext()
        {
            var incomingRequest = WebOperationContext.Current.IncomingRequest;

            //初始化AuthenticationContext
            AuthenticationToken authToken = new AuthenticationToken(incomingRequest.UriTemplateMatch.RequestUri, incomingRequest.UriTemplateMatch.QueryParameters, incomingRequest.Method);
            AuthenticationContext.Current = new AuthenticationContext(authToken)
            {
                //赋值TokenId
                TokenId = incomingRequest.UriTemplateMatch.QueryParameters["tokenId"]
            };

            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.Request.Cookies != null)
                    AuthenticationContext.Current.Token.Cookies = HttpContext.Current.Request.Cookies;
            }
            else
            {
                string cookie = incomingRequest.Headers[HttpRequestHeader.Cookie];
                SetCookie(cookie);
            }
        }

        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="cookie"></param>
        private static void SetCookie(string cookie)
        {
            if (!string.IsNullOrEmpty(cookie))
            {
                HttpCookieCollection collection = new HttpCookieCollection();
                string[] cookies = cookie.Split(';');
                HttpCookie cook = null;
                foreach (string e in cookies)
                {
                    if (!string.IsNullOrEmpty(e))
                    {
                        string[] values = e.Split(new char[] { '=' }, 2);
                        if (values.Length == 2)
                        {
                            cook = new HttpCookie(values[0], values[1]);
                        }
                        collection.Add(cook);
                    }
                }

                AuthenticationContext.Current.Token.Cookies = collection;
            }
        }

        /// <summary>
        /// 进行认证
        /// </summary>
        /// <returns></returns>
        public static RESTfulResult Authorize()
        {
            //初始化上下文
            InitializeContext();

            var response = WebOperationContext.Current.OutgoingResponse;
            response.StatusCode = HttpStatusCode.Unauthorized;

            AuthType authType = AuthType.Cookie;

            //判断认证类型
            if (AuthenticationContext.Current != null)
            {
                var context = AuthenticationContext.Current;
                if (context.Token.Parameters["uid"] != null && context.Token.Parameters["pwd"] != null)
                {
                    authType = AuthType.UidPwd;
                }
                else if (context.Token.Parameters["oauth_token"] != null)
                {
                    authType = AuthType.OAuth;
                }
            }

            //进行认证处理
            var result = new RESTfulResult
            {
                Code = (int)RESTfulCode.AUTH_FAULT,
                Message = "All authentication fail!"
            };

            try
            {
                if (auths.Count == 0)
                {
                    result.Code = (int)RESTfulCode.AUTH_ERROR;
                    result.Message = "No any authentication!";
                    return result;
                }

                //获取指定的认证;
                var authlist = auths.Where(p => p.AuthType == authType).ToList();
                List<string> errors = new List<string>();
                bool isAuthentication = false;

                //如果配置了服务
                foreach (IAuthentication auth in authlist)
                {
                    if (auth.Authorize())
                    {
                        //认证成功
                        result.Code = (int)RESTfulCode.OK;
                        result.Message = authType + " authentication success!";

                        isAuthentication = true;
                        break;
                    }
                    else
                    {
                        errors.Add(result.Message);
                    }
                }

                if (!isAuthentication)
                {
                    if (result.Code == 0) result.Code = (int)RESTfulCode.AUTH_ERROR;
                    if (errors.Count == 0) errors.Add("No any " + authType + " authentication!");
                    result.Message = string.Join(" | ", errors.ToArray());
                }
            }
            catch (AuthenticationException ex)
            {
                result.Code = ex.Code;
                result.Message = ex.Message;
            }
            catch (Exception ex)
            {
                result.Code = (int)RESTfulCode.AUTH_ERROR;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
