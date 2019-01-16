using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Twinkle.Framework.SignalR
{
    public class OnlineClientManager : IOnlineClientManager
    {

        protected ConcurrentDictionary<string, IOnlineClient> Clients { get; }

        protected readonly object SyncObj = new object();

        public OnlineClientManager()
        {
            Clients = new ConcurrentDictionary<string, IOnlineClient>();
        }
        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="client"></param>
        public void Add(IOnlineClient client)
        {
            lock (SyncObj)
            {
                Clients[client.ConnectionId] = client;
            }
        }

        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="connectionId"></param>
        public bool Remove(string connectionId)
        {
            lock (SyncObj)
            {
                return Clients.TryRemove(connectionId, out IOnlineClient client);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public IOnlineClient GetByConnectionId(string connectionId)
        {
            lock (SyncObj)
            {
                if (Clients.TryGetValue(connectionId, out IOnlineClient client))
                {
                    return client;
                }
                else
                {
                    return default(IOnlineClient);
                }
            }
        }

        /// <summary>
        /// 获取所有客户端
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<IOnlineClient> GetAllClients()
        {
            lock (SyncObj)
            {
                return Clients.Values.ToImmutableList();
            }
        }

        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="TenantId"></param>
        /// <returns></returns>
        public IOnlineClient GetClientById(string UserId, string TenantId)
        {
            lock (SyncObj)
            {
                return GetAllClients()
                .Where(c => (c.UserId == UserId && c.TenantId == TenantId)).FirstOrDefault();
            }
        }


        /// <summary>
        /// 获取所有ID关联客户端
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="TenantId"></param>
        /// <returns></returns>
        public IReadOnlyList<IOnlineClient> GetAllByUserId(string UserId, string TenantId)
        {
            return GetAllClients()
                .Where(c => (c.UserId == UserId && c.TenantId == TenantId)).ToImmutableList();
        }
    }
}
