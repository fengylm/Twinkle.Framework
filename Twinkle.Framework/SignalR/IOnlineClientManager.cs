using System.Collections.Generic;

namespace Twinkle.Framework.SignalR
{
    public interface IOnlineClientManager
    {
        /// <summary>
        /// 添加客户端
        /// </summary>
        /// <param name="client"></param>
        void Add(IOnlineClient client);

        /// <summary>
        /// 移除客户端
        /// </summary>
        /// <param name="connectionId"></param>
        bool Remove(string connectionId);


        IOnlineClient GetByConnectionId(string connectionId);

        /// <summary>
        /// 获取客户端
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="TenantId"></param>
        /// <returns></returns>
        IOnlineClient GetClientById(string UserId, int? TenantId);

        /// <summary>
        /// 获取所有客户端
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<IOnlineClient> GetAllClients();

        /// <summary>
        /// 获取所有ID关联客户端
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="TenantId"></param>
        /// <returns></returns>
        IReadOnlyList<IOnlineClient> GetAllByUserId(string UserId, int? TenantId);
    }
}
