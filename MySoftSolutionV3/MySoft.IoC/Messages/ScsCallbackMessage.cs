﻿using System;
using MySoft.Communication.Scs.Communication.Messages;

namespace MySoft.IoC.Messages
{
    /// <summary>
    /// 回调消息
    /// </summary>
    [Serializable]
    public class ScsCallbackMessage : ScsMessage
    {
        public CallbackMessage MessageValue { get; set; }

        public ScsCallbackMessage()
        {
            //TO DO
        }

        public ScsCallbackMessage(CallbackMessage value)
        {
            this.MessageValue = value;
        }
    }
}
