﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// RESTful客户端
    /// </summary>
    public class RESTfulFactory
    {
        /// <summary>
        /// 创建一个客户端
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static RESTfulFactory Create(string url)
        {
            return new RESTfulFactory(url);
        }

        /// <summary>
        /// 创建一个客户端
        /// </summary>
        /// <param name="url"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static RESTfulFactory Create(string url, DataFormat format)
        {
            return new RESTfulFactory(url, format);
        }

        private string url;
        private DataFormat format = DataFormat.JSON;
        private int timeout = 5 * 60;

        /// <summary>
        /// 数据格式
        /// </summary>
        public DataFormat Format
        {
            get { return format; }
            set { format = value; }
        }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        /// <summary>
        /// RESTfulClient实例化
        /// </summary>
        protected RESTfulFactory(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url不能为空值！");
            }

            this.url = url;
        }

        /// <summary>
        /// RESTfulClient实例化
        /// </summary>
        /// <param name="url"></param>
        /// <param name="format"></param>
        protected RESTfulFactory(string url, DataFormat format)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url不能为空值！");
            }

            this.url = url;
            this.format = format;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>()
        {
            return GetChannel<IServiceInterfaceType>(new Token());
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <returns></returns>
        public IServiceInterfaceType GetChannel<IServiceInterfaceType>(Token token)
        {
            Exception ex = new ArgumentException("Generic parameter type - 【" + typeof(IServiceInterfaceType).FullName
                + "】 must be an interface marked with PublishKindAttribute.");


            PublishKindAttribute kindattr = null;
            if (!typeof(IServiceInterfaceType).IsInterface)
            {
                throw ex;
            }
            else
            {
                bool markedWithServiceContract = false;
                var attr = CoreHelper.GetTypeAttribute<PublishKindAttribute>(typeof(IServiceInterfaceType));
                if (attr != null)
                {
                    markedWithServiceContract = true;
                }

                kindattr = attr;
                attr = null;

                if (!markedWithServiceContract)
                {
                    throw ex;
                }
            }

            var serviceType = typeof(IServiceInterfaceType);
            var handler = new RESTfulInvocationHandler(url, kindattr, token, format, timeout);
            var dynamicProxy = ProxyFactory.GetInstance().Create(handler, serviceType, true);

            return (IServiceInterfaceType)dynamicProxy;
        }

        #region 指定返回类型

        #region 不带参数方式

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name)
        {
            return Invoke<TResult>(name, HttpMethod.GET);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, HttpMethod method)
        {
            return Invoke<TResult>(name, new Token(), method);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, Token token)
        {
            return Invoke<TResult>(name, token, HttpMethod.GET);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, Token token, HttpMethod method)
        {
            RESTfulParameter parameter = new RESTfulParameter(name, method, format);
            parameter.Token = token;

            RESTfulRequest request = new RESTfulRequest(parameter);
            if (!string.IsNullOrEmpty(url)) request.Url = url;

            return request.GetResponse<TResult>();
        }

        #endregion

        #region 带参数方式(字典)

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, IDictionary<string, object> item)
        {
            return Invoke<TResult>(name, item, HttpMethod.GET);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, IDictionary<string, object> item, HttpMethod method)
        {
            return Invoke<TResult>(name, item, new Token(), method);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, IDictionary<string, object> item, Token token)
        {
            return Invoke<TResult>(name, item, token, HttpMethod.GET);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, IDictionary<string, object> item, Token token, HttpMethod method)
        {
            if (item == null)
            {
                throw new ArgumentException("item不能为null！");
            }

            RESTfulParameter parameter = new RESTfulParameter(name, method, format);
            parameter.Token = token;

            if (method != HttpMethod.POST)
            {
                //添加参数
                parameter.AddParameter(item.Keys.ToArray(), item.Values.ToArray());
            }
            else
            {
                parameter.DataObject = item;
            }

            RESTfulRequest request = new RESTfulRequest(parameter);
            if (!string.IsNullOrEmpty(url)) request.Url = url;

            return request.GetResponse<TResult>();
        }

        #endregion

        #region 带参数方式(对象)

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, object item)
        {
            return Invoke<TResult>(name, item, HttpMethod.GET);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, object item, HttpMethod method)
        {
            return Invoke<TResult>(name, item, new Token(), method);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, object item, Token token)
        {
            return Invoke<TResult>(name, item, token, HttpMethod.GET);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public TResult Invoke<TResult>(string name, object item, Token token, HttpMethod method)
        {
            if (item == null || !item.GetType().IsClass)
            {
                throw new ArgumentException("item不能为null，且必须为类对象，可以是匿名类对象！");
            }

            RESTfulParameter parameter = new RESTfulParameter(name, method, format);
            parameter.Token = token;

            if (method != HttpMethod.POST)
            {
                //添加参数
                parameter.AddParameter(item);
            }
            else
            {
                var collection = new Dictionary<string, object>();

                //添加参数
                var plist = CoreHelper.GetPropertiesFromType(item.GetType());
                for (int index = 0; index < plist.Length; index++)
                {
                    collection[plist[index].Name] = CoreHelper.GetPropertyValue(item, plist[index]);
                }

                parameter.DataObject = collection;
            }

            RESTfulRequest request = new RESTfulRequest(parameter);
            if (!string.IsNullOrEmpty(url)) request.Url = url;

            return request.GetResponse<TResult>();
        }

        #endregion

        #endregion

        #region 不指定返回类型

        #region 不带参数方式

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public JToken Invoke(string name)
        {
            return Invoke<JToken>(name);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public JToken Invoke(string name, HttpMethod method)
        {
            return Invoke<JToken>(name, method);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public JToken Invoke(string name, Token token)
        {
            return Invoke<JToken>(name, token);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="token"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public JToken Invoke(string name, Token token, HttpMethod method)
        {
            return Invoke<JToken>(name, token, method);
        }

        #endregion

        #region 带参数方式(字典)

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public JToken Invoke(string name, IDictionary<string, object> item)
        {
            return Invoke<JToken>(name, item);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public JToken Invoke(string name, IDictionary<string, object> item, HttpMethod method)
        {
            return Invoke<JToken>(name, item, method);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public JToken Invoke(string name, IDictionary<string, object> item, Token token)
        {
            return Invoke<JToken>(name, item, token);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public JToken Invoke(string name, IDictionary<string, object> item, Token token, HttpMethod method)
        {
            return Invoke<JToken>(name, item, token, method);
        }

        #endregion

        #region 带参数方式(对象)

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public JToken Invoke(string name, object item)
        {
            return Invoke<JToken>(name, item);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public JToken Invoke(string name, object item, HttpMethod method)
        {
            return Invoke<JToken>(name, item, method);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public JToken Invoke(string name, object item, Token token)
        {
            return Invoke<JToken>(name, item, token);
        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public JToken Invoke(string name, object item, Token token, HttpMethod method)
        {
            return Invoke<JToken>(name, item, token, method);
        }

        #endregion

        #endregion
    }
}
