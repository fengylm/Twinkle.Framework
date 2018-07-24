using StackExchange.Redis;
using System;

namespace Twinkle.Framework.Cache
{
    internal class RedisConnection
    {
        internal static RedisConfig Config { get; set; }

        private static readonly object _lock = new object();

        private static ConnectionMultiplexer instance;

        private static readonly object _msgLock = new object();

        private static ConnectionMultiplexer message;

        /// <summary>
        /// 单例获取数据缓存单例
        /// </summary>
        internal static ConnectionMultiplexer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null || !instance.IsConnected)
                        {
                            if(Config==null)
                            {
                                throw new ArgumentNullException("未初始化RedisConfig实例对象");
                            }
                            instance = GetDataConnection();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 获取消息通讯单例
        /// </summary>
        internal static ConnectionMultiplexer Message
        {
            get
            {
                if (message == null)
                {
                    lock (_msgLock)
                    {
                        if (message == null || !message.IsConnected)
                        {
                            if (Config == null)
                            {
                                throw new ArgumentNullException("未初始化RedisConfig实例对象");
                            }
                            message = GetMessageConnection();
                        }
                    }
                }
                return message;
            }
        }

        private static ConnectionMultiplexer GetDataConnection()
        {
            #region 设置Redis集群
            ConfigurationOptions option = new ConfigurationOptions
            {
                ServiceName = Config.ServerName,
                Proxy = Proxy.Twemproxy,
                AbortOnConnectFail = true,
                Password = Config.Password,
                AllowAdmin = true,
            };
            if(Config.ServerHosts?.Length>0)
            {
                foreach (var host in Config.ServerHosts)
                {
                    option.EndPoints.Add(host);
                }
            }

            var connect = ConnectionMultiplexer.Connect(option);
            #endregion


            #region 设置Sentinel集群

            if (Config.SentinelHosts?.Length > 0)
            {
                ConfigurationOptions sentinelConfig = new ConfigurationOptions();
                sentinelConfig.ServiceName = Config.ServerName;
           
                foreach (var host in Config.SentinelHosts)
                {
                    sentinelConfig.EndPoints.Add(host);
                }

                sentinelConfig.TieBreaker = "";//这行在sentinel模式必须加上
                sentinelConfig.CommandMap = CommandMap.Sentinel;
                sentinelConfig.DefaultVersion = new Version(3, 0);
                ConnectionMultiplexer.Connect(sentinelConfig);
            }
            #endregion

            return connect;
        }
        private static ConnectionMultiplexer GetMessageConnection()
        {

            ConfigurationOptions option = new ConfigurationOptions
            {
                ServiceName = "messageBus",
                Proxy = Proxy.None,
                AbortOnConnectFail = true,
                AllowAdmin = true,
            };
            if (Config.ServerHosts?.Length > 0)
            {
                foreach (var host in Config.ServerHosts)
                {
                    option.EndPoints.Add(host);
                }
            }

            var connect = ConnectionMultiplexer.Connect(option);
            return connect;
        }
    }
}
