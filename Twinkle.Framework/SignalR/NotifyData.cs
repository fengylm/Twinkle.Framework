using System;

namespace Twinkle.Framework.SignalR
{
    [Serializable]
    public class NotifyData
    {
        /// <summary>
        /// 订阅标识,所有订阅该标识的客户端都会收到消息
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// 消息数据
        /// </summary>
        public object Data { get; set; }

    }
}
