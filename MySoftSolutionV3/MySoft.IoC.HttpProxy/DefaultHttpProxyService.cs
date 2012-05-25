﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MySoft.Auth;
using MySoft.Security;

namespace MySoft.IoC.HttpProxy
{
    /// <summary>
    /// 默认的http代理服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DefaultHttpProxyService : IHttpProxyService
    {
        const string HTTP_PROXY_API = "{0}/{1}";
        const string HTTP_PROXY_URL = "{0}/{1}?{2}";
        private string proxyServer;
        private HttpHelper helper;
        private IList<ServiceItem> services;

        // TODO: Implement the collection resource that will contain the SampleItem instances
        public DefaultHttpProxyService()
        {
            var url = ConfigurationManager.AppSettings["HttpProxyServer"];
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("Http proxy server can't for empty.");
            else
                proxyServer = url;

            this.helper = new HttpHelper(Encoding.UTF8, 30);
            this.services = new List<ServiceItem>();

            //读取服务信息
            this.ReaderService();
        }

        /// <summary>
        /// 读取服务
        /// </summary>
        private void ReaderService()
        {
            //数据缓存1分钟
            var url = string.Format(HTTP_PROXY_API, proxyServer, "api");
            var jsonString = helper.Reader(url, 60);

            //将数据反系列化成对象
            this.services = SerializationManager.DeserializeJson<IList<ServiceItem>>(jsonString);
        }

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <returns>字节数据流</returns>
        public Stream GetTextEntry(string name)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            var query = request.UriTemplateMatch.QueryParameters;

            //响应格式
            response.ContentType = "application/json;charset=utf-8";

            //认证用户信息
            ServiceItem service;
            var header = new WebHeaderCollection();
            var jsonString = AuthorizeMethod(name, header, out service);

            //如果jsonString为null，则继续处理
            if (string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    //数据缓存5秒
                    var url = string.Empty;
                    if (query.Count > 0)
                    {
                        var parameters = HttpUtility.UrlDecode(query.ToString());
                        url = string.Format(HTTP_PROXY_URL, proxyServer, name, parameters);
                    }
                    else
                    {
                        url = string.Format(HTTP_PROXY_API, proxyServer, name);
                    }

                    jsonString = helper.Reader(url, 5, header);

                    if (service != null && service.TypeString)
                    {
                        //如果返回是字符串类型，则设置为文本返回
                        response.ContentType = "text/plain;charset=utf-8";
                    }

                    //判断是否需要回调
                    var callback = query["callback"];

                    if (string.IsNullOrEmpty(callback))
                    {
                        //如果值为空或null
                        if (string.IsNullOrEmpty(jsonString))
                        {
                            return new MemoryStream();
                        }

                        var bytes = Encoding.UTF8.GetBytes(jsonString);
                        string etagToken = MD5.HexHash(bytes);
                        response.ETag = etagToken;
                        var IfNoneMatch = request.Headers["If-None-Match"];
                        if (IfNoneMatch != null && IfNoneMatch == etagToken)
                        {
                            response.StatusCode = HttpStatusCode.NotModified;
                            //request.IfModifiedSince.HasValue ? request.IfModifiedSince.Value : 
                            var IfModifiedSince = request.Headers["If-Modified-Since"];
                            response.LastModified = IfModifiedSince == null ? DateTime.Now : Convert.ToDateTime(IfModifiedSince);
                            return new MemoryStream();
                        }
                        else
                        {
                            response.LastModified = DateTime.Now;
                        }
                    }
                    else
                    {
                        //输出为javascript格式数据
                        response.ContentType = "application/javascript;charset=utf-8";
                        jsonString = string.Format("{0}({1});", callback, jsonString ?? "{}");
                    }
                }
                catch (WebException ex)
                {
                    var rep = (ex.Response as HttpWebResponse);
                    var stream = rep.GetResponseStream();
                    using (var sr = new StreamReader(stream))
                    {
                        jsonString = sr.ReadToEnd();
                    }

                    response.StatusCode = rep.StatusCode;
                    response.StatusDescription = rep.StatusDescription;
                }
            }

            //转换成utf8返回
            var buffer = Encoding.UTF8.GetBytes(jsonString);
            return new MemoryStream(buffer);
        }

        /// <summary>
        /// POST入口
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <returns>字节数据流</returns>
        public Stream PostTextEntry(string name, Stream stream)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            var query = request.UriTemplateMatch.QueryParameters;

            //响应格式
            response.ContentType = "application/json;charset=utf-8";

            //认证用户信息
            ServiceItem item;
            var header = new WebHeaderCollection();
            var jsonString = AuthorizeMethod(name, header, out item);

            //如果jsonString为null，则继续处理
            if (string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    var postValue = string.Empty;
                    using (var sr = new StreamReader(stream))
                    {
                        postValue = sr.ReadToEnd();
                    }

                    var url = string.Empty;
                    if (query.Count > 0)
                    {
                        var parameters = HttpUtility.UrlDecode(query.ToString());
                        url = string.Format(HTTP_PROXY_URL, proxyServer, name, parameters);
                    }
                    else
                    {
                        url = string.Format(HTTP_PROXY_API, proxyServer, name);
                    }

                    jsonString = helper.Poster(url, postValue, header);
                    if (item != null && item.TypeString)
                    {
                        //如果返回是字符串类型，则设置为文本返回
                        response.ContentType = "text/plain;charset=utf-8";
                    }
                }
                catch (WebException ex)
                {
                    var rep = (ex.Response as HttpWebResponse);
                    var output = rep.GetResponseStream();
                    using (var sr = new StreamReader(output))
                    {
                        jsonString = sr.ReadToEnd();
                    }

                    response.StatusCode = rep.StatusCode;
                    response.StatusDescription = rep.StatusDescription;
                }
            }

            //转换成utf8返回
            var buffer = Encoding.UTF8.GetBytes(jsonString);
            return new MemoryStream(buffer);
        }

        /// <summary>
        /// GET入口
        /// </summary>
        /// <returns>字节数据流</returns>
        public Stream GetDocument()
        {
            return GetDocumentFromKind(null);
        }

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="kind"></param>
        /// <returns>字节数据流</returns>
        public Stream GetDocumentFromKind(string kind)
        {
            var request = WebOperationContext.Current.IncomingRequest;
            var response = WebOperationContext.Current.OutgoingResponse;
            var method = "help";
            if (!string.IsNullOrEmpty(kind)) method += ("/" + kind);

            //文档缓存1分钟
            var url = string.Format(HTTP_PROXY_API, proxyServer, method);
            string html = helper.Reader(url, 60);

            //转换成utf8返回
            response.ContentType = "text/html;charset=utf-8";
            var regex = new Regex(@"<title>([\s\S]+) 处的操作</title>", RegexOptions.IgnoreCase);
            if (regex.IsMatch(html))
            {
                url = string.Format("http://{0}/", request.UriTemplateMatch.RequestUri.Authority);
                html = html.Replace(regex.Match(html).Result("$1"), url);
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(html));
        }

        private string AuthorizeMethod(string name, WebHeaderCollection header, out ServiceItem service)
        {
            service = null;
            var response = WebOperationContext.Current.OutgoingResponse;

            //检测服务名称
            if (name == "favicon.ico")
            {
                response.StatusCode = HttpStatusCode.NotFound;
                var item = new HttpProxyResult { Code = (int)response.StatusCode, Message = "Service 【" + name + "】 not found." };
                return SerializeJson(item);
            }
            else if (!services.Any(p => string.Compare(p.Name, name, true) == 0))
            {
                response.StatusCode = HttpStatusCode.NotFound;
                var item = new HttpProxyResult { Code = (int)response.StatusCode, Message = "Method 【" + name + "】 not found." };
                return SerializeJson(item);
            }
            else
            {
                #region 进行认证处理

                service = services.Single(p => string.Compare(p.Name, name, true) == 0);

                //认证处理
                if (service.Authorized)
                {
                    var result = AuthorizeHeader(header);
                    if (result.Code == (int)HttpStatusCode.OK)
                        return null;
                    else
                        return SerializeJson(result);
                }

                #endregion
            }

            return null;
        }

        private HttpProxyResult AuthorizeHeader(WebHeaderCollection header)
        {
            var response = WebOperationContext.Current.OutgoingResponse;
            response.StatusCode = HttpStatusCode.Unauthorized;

            try
            {
                var token = Authorize();
                if (token.Succeed && !string.IsNullOrEmpty(token.Name))
                {
                    header["X-AuthParameter"] = token.Name;
                    response.StatusCode = HttpStatusCode.OK;
                    return new HttpProxyResult { Code = (int)response.StatusCode };
                }
                else
                {
                    return new HttpProxyResult { Code = (int)response.StatusCode, Message = "Unauthorized or authorize name is empty." };
                }
            }
            catch (Exception ex)
            {
                return new HttpProxyResult { Code = (int)response.StatusCode, Message = "Unauthorized - " + ex.Message };
            }
        }

        /// <summary>
        /// 系列化数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private string SerializeJson(object item)
        {
            return SerializationManager.SerializeJson(item);
        }

        /// <summary>
        /// 进行认证处理
        /// </summary>
        /// <returns></returns>
        protected virtual AuthorizeToken Authorize()
        {
            //返回认证失败
            return new AuthorizeToken
            {
                Succeed = false,
                Name = "Unknown"
            };
        }
    }
}
