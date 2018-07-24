using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Twinkle.Framework.Mvc;
using Twinkle.Framework.Security;

namespace Twinkle.Framework.SignalR
{
    public class SRHub:Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (this.Context.GetHttpContext().Request.Cookies.ContainsKey("access-token"))
            {
                string token = this.Context.GetHttpContext().Request.Cookies["access-token"];
                object userData = TwinkleContext.GetService<JWT>().GetUserData(token);
                if (userData != null)
                {
                    HubClient client = new HubClient
                    {
                        ConnectionId = this.Context.ConnectionId,
                        AccountId = JToken.Parse(userData.ToString()).Value<string>("uid"),
                    };
                    AddClient((dynamic)this,client);
                }
            }
            await base.OnConnectedAsync();
        }

        private void AddClient<T>(T hub, HubClient client) where T : Hub
        {
            SRServer<T>.Instance.AddClient(client);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string token = this.Context.GetHttpContext().Request.Cookies["access-token"];
            object userData = TwinkleContext.GetService<JWT>().GetUserData(token);
            if (userData != null)
            {
                RemoveClient((dynamic)this, JToken.Parse(userData.ToString()).Value<string>("uid"));
            }
            await base.OnDisconnectedAsync(exception);
        }

        private void RemoveClient<T>(T hub, string AccountId) where T : Hub
        {
            SRServer<T>.Instance.RemoveClient(AccountId);
        }
    }
}
