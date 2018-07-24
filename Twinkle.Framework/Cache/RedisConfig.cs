using System;
using System.Collections.Generic;
using System.Text;

namespace Twinkle.Framework.Cache
{
    public class RedisConfig
    {
        /// <summary>
        /// 服务名称,启用redis集群的时候必须使用
        /// </summary>
        public string ServerName { get; set; }
        /// <summary>
        /// redis服务器地址
        /// </summary>
        public string[] ServerHosts { get; set; }
        /// <summary>
        /// redis哨兵服务器地址
        /// </summary>
        public string[] SentinelHosts { get; set; }
        /// <summary>
        /// redis密码,在使用集群时,集群redis服务器要配置相同的密码
        /// </summary>
        public string Password { get; set; }
    }
}
