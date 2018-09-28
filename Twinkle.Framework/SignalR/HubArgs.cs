using System;
using System.Collections.Generic;
using System.Text;

namespace Twinkle.Framework.SignalR
{
    public class HubArgs
    {
        /// <summary>
        /// 订阅名称
        /// </summary>
        public string PublishName { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public dynamic PublishArgs { get; set; }

        /// <summary>
        /// 客户端连接id
        /// </summary>
        public string ConnectionId { get; set; }
    }
}
