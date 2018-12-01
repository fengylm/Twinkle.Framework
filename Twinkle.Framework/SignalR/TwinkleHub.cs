using Twinkle.Framework.Security.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Twinkle.Framework.SignalR
{
    [Authorize]
    public class TwinkleHub : Hub
    {
        private readonly IOnlineClientManager mOnlineClientManager;
        public TwinkleHub(IOnlineClientManager onlineClientManager)
        {
            mOnlineClientManager = onlineClientManager;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var client = mOnlineClientManager.GetByConnectionId(Context.ConnectionId);
            if (client == null)
            {
                client = CreateClientForCurrentConnection();
                mOnlineClientManager.Add(client);
            }
        }




        private IOnlineClient CreateClientForCurrentConnection()
        {
            AuthUser user = JToken.Parse(Context.GetHttpContext().Session.GetString("AuthUser")).ToObject<AuthUser>();

            return new OnlineClient
            {
                ConnectionId = Context.ConnectionId,
                TenantId = user.TenantId,
                UserId = user.UserId
            };
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            try
            {
                mOnlineClientManager.Remove(Context.ConnectionId);
            }
            catch
            {
            }
        }
    }
}
