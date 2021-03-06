﻿using System;

namespace MySoft.IoC
{
    /// <summary>
    /// 警告异常信息
    /// </summary>
    [Serializable]
    public class WarningException : IoCException
    {
        /// <summary>
        /// 普通异常的构造方法
        /// </summary>
        /// <param name="message"></param>
        public WarningException(string message)
            : base(message) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info">存储对象序列化和反序列化所需的全部数据</param>
        /// <param name="context">描述给定的序列化流的源和目标，并提供一个由调用方定义的附加上下文</param>
        protected WarningException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
