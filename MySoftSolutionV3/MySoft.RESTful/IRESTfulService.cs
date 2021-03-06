﻿using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MySoft.RESTful
{
    /// <summary>
    /// RESTful接口
    /// </summary>
    [ServiceContract]
    public interface IRESTfulService
    {
        /// <summary>
        /// 内容对象
        /// </summary>
        IRESTfulContext Context { get; set; }

        #region text 方式

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="kind">发布的业务分类</param>
        /// <param name="method">发布的业务方法</param>
        /// <returns>字节数据流</returns>
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "{kind}.{method}.text")]
        Stream GetTextEntry(string kind, string method);

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="kind">发布的业务分类</param>
        /// <param name="method">发布的业务方法</param>
        /// <returns>字节数据流</returns>
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "{kind}.{method}.html")]
        Stream GetHtmlEntry(string kind, string method);

        #endregion

        #region json 方式

        /// <summary>
        /// POST入口
        /// </summary>
        /// <param name="kind">发布的分类</param>
        /// <param name="method">发布的方法名称</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>字节数据流</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "{kind}.{method}.json")]
        Stream PostJsonEntry(string kind, string method, Stream parameters);

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="kind">发布的业务分类</param>
        /// <param name="method">发布的业务方法</param>
        /// <returns>字节数据流</returns>
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "{kind}.{method}.json")]
        Stream GetJsonEntry(string kind, string method);

        #endregion

        #region xml 方式

        /// <summary>
        /// POST入口
        /// </summary>
        /// <param name="kind">发布的分类</param>
        /// <param name="method">发布的方法名称</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>字节数据流</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "{kind}.{method}.xml")]
        Stream PostXmlEntry(string kind, string method, Stream parameters);

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="kind">发布的业务分类</param>
        /// <param name="method">发布的业务方法</param>
        /// <returns>字节数据流</returns>
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "{kind}.{method}.xml")]
        Stream GetXmlEntry(string kind, string method);

        #endregion

        /// <summary>
        /// 带有回调地址的Get入口
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "{kind}.{method}.jsonp")]
        Stream GetEntryCallBack(string kind, string method);

        /// <summary>
        /// 发布接口的实时html文档
        /// </summary>
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "help")]
        Stream GetMethodHtml();

        /// <summary>
        /// 发布接口的实时html文档
        /// </summary>
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "help/{kind}")]
        Stream GetMethodHtmlFromKind(string kind);

        /// <summary>
        /// 发布接口的实时html文档
        /// </summary>
        [OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "help/{kind}/{method}")]
        Stream GetMethodHtmlFromKindAndMethod(string kind, string method);
    }
}
