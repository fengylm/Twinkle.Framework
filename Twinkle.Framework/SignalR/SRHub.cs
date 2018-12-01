using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Twinkle.Framework.SignalR
{
    [Authorize]
    public class SRHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (this.Context.GetHttpContext().Request.Cookies.ContainsKey("accessToken"))
            {
                string token = this.Context.GetHttpContext().Request.Cookies["accessToken"];
                User user = TwinkleContext.GetService<TokenAuthManager>().GetUser(token);
                if (user != null && TwinkleContext.GetService<TokenAuthManager>().IsValid(token))
                {
                    HubClient client = new HubClient
                    {
                        ConnectionId = this.Context.ConnectionId,
                        AccountId = user.UserId
                    };
                    AddClient((dynamic)this, client);
                }
            }
            else
            {
                throw new Exception("授权无效,或已过期");
            }
            await base.OnConnectedAsync();
        }

        private void AddClient<T>(T hub, HubClient client) where T : Hub
        {
            SRService<T>.Instance.AddClient(client);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string token = this.Context.GetHttpContext().Request.Cookies["access-token"];
            object userData = TwinkleContext.GetService<TokenAuthManager>().GetUser(token);
            if (userData != null)
            {
                RemoveClient((dynamic)this, JToken.Parse(userData.ToString()).Value<string>("uid"));
            }
            await base.OnDisconnectedAsync(exception);
        }

        private void RemoveClient<T>(T hub, string AccountId) where T : Hub
        {
            SRService<T>.Instance.RemoveClient(AccountId);
        }
    }
}
