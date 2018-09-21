using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twinkle.Framework.Mvc;

namespace Twinkle.Framework.SignalR
{
    public sealed class SRService<T> where T : Hub
    {
        #region 私有变量
        private readonly static Lazy<SRService<T>> _instance = new Lazy<SRService<T>>(() => new SRService<T>(TwinkleContext.GetService<IHubContext<T>>(), TwinkleContext.GetService<ILoggerFactory>()));
        private ConcurrentDictionary<string, HubClient> _clients = new ConcurrentDictionary<string, HubClient>();
        private ConcurrentDictionary<string, Action<dynamic>> _methods = new ConcurrentDictionary<string, Action<dynamic>>();
        private readonly IHubContext<T> _context;
        private readonly ILoggerFactory _logger;
        public delegate void SubscribeHandler(HubArgs Args);
        public event SubscribeHandler OnSubscribe;
        #endregion

        internal Task Dispatcher(string PublishName, dynamic Args, string ConnectionId)
        {
            if (OnSubscribe == null)
            {
                return Task.CompletedTask;
            }
            var task = Task.Run(() =>
            {
                OnSubscribe.Invoke(new HubArgs
                {
                    PublishName = PublishName,
                    PublishArgs = Args,
                    ConnectionId = ConnectionId
                });
            });

            return Task.WhenAll(new Task[] { task });
        }

        public SRService(IHubContext<T> Context, ILoggerFactory Logger)
        {
            _context = Context;
            _logger = Logger;
        }

        public static SRService<T> Instance
        {
            get
            {
                return _instance.Value;
            }
        }
        /// <summary>
        /// 获取客户端集合
        /// </summary>
        public List<HubClient> Clients
        {
            get
            {
                return _clients.Select((item) => { return item.Value; }).ToList();
            }
        }

        #region 客户端管理
        internal void AddClient(HubClient client)
        {

            //   _context.Groups.AddToGroupAsync(client.ConnectionId, client.GroupId);
            _clients.AddOrUpdate(client.AccountId, client, (key, oldValue) =>
            {
                //if (oldValue.ConnectionId != client.ConnectionId)
                //{
                //    //对于已经更新过的客户端信息,需要移除旧的分组信息
                //    _context.Groups.RemoveFromGroupAsync(oldValue.ConnectionId, oldValue.GroupId);
                //}
                return client;
            });


        }

        internal void RemoveClient(string AccountId)
        {
            HubClient client;

            if (_clients.TryRemove(AccountId, out client))
            {
                // _context.Groups.RemoveFromGroupAsync(client.ConnectionId, client.GroupId);
            }
        }
        #endregion

        #region 消息发送
        /// <summary>
        /// 发送消息给指定用户
        /// </summary>
        /// <param name="AccountId">接收消息的用户ID</param>
        /// <param name="Message">消息对象</param>
        /// <param name="Listener">前端监听对象,就是前台connection.on的第一个参数</param>

        public Task Send(string AccountId,SignalrResponse Message, string Listener = "ServerMessage")
        {
            return _context.Clients.Client(_clients.Where(c => c.Key == AccountId).FirstOrDefault().Value?.ConnectionId).SendCoreAsync(Listener, new object[] { Message });
        }
        /// <summary>
        /// 通过ConnectionId 发送给指定对象
        /// </summary>
        /// <param name="ConnectionId">当前客户端连接ID</param>
        /// <param name="Message">消息对象</param>
        /// <param name="Listener">前端监听对象,就是前台connection.on的第一个参数</param>
        /// <returns></returns>
        public Task SendByConnectionId(string ConnectionId, SignalrResponse Message, string Listener = "ServerMessage")
        {
            return _context.Clients.Client(ConnectionId).SendCoreAsync(Listener, new object[] { Message });
        }

        /// <summary>
        /// 发送消息给所有用户
        /// </summary>
        /// <param name="Message">消息对象</param>
        /// <param name="Listener">前端监听对象,就是前台connection.on的第一个参数</param>
        /// <param name="ExceptAccountIds">群发时,需要屏蔽的用户ID</param>
        public Task SendAll(SignalrResponse Message, string Listener="ServerMessage", string[] ExceptAccountIds = null)
        {
            //signalr自带的客户客户端管理模块是不和用户认证关联的,所以同一个账号在两个浏览器登陆的时候
            //后登陆的不会把前登陆的覆盖,在signalr中是会被识别为两个客户端的
            //所以这里使用轮询自己实现的客户端管理来处理群发
            var tasks = new List<Task>();
            foreach (var item in _clients)
            {
                if (!(ExceptAccountIds?.Contains(item.Value.AccountId) == true))
                {
                    tasks.Add(SendByConnectionId(item.Value.ConnectionId, Message, Listener));
                }
            }
            return Task.WhenAll(tasks);
        }

        ///// <summary>
        ///// 发送消息给指定组
        ///// </summary>
        ///// <param name="GroupId">组ID</param>
        ///// <param name="Listener">前端监听对象,就是前台connection.on的第一个参数</param>
        ///// <param name="Message">消息对象</param>
        ///// <param name="ExceptAccountIds">组群发时,需要屏蔽的用户ID</param>
        //public Task SendGroup(string GroupId, string Listener, SignalrResponse Message, string[] ExceptAccountIds = null)
        //{
        //    //组群发不accountId排除发送,自己判断实现
        //    var tasks = new List<Task>();
        //    foreach (var item in _clients)
        //    {
        //        if (!(ExceptAccountIds?.Contains(item.Value.AccountId) == true) && item.Value.GroupId == GroupId)
        //        {
        //            tasks.Add(SendByConnectionId(item.Value.ConnectionId, Listener, Message));
        //        }
        //    }
        //    return Task.WhenAll(tasks);
        //}

        #endregion
    }

    public class HubClient
    {
        public string ConnectionId { get; set; }

        public string AccountId { get; set; }

    }

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

    public class SignalrResponse
    {
        public string Channel { get; set; }

        public object Body { get; set; }
    }
}
