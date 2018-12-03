using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Twinkle.Framework.Extensions;

namespace Twinkle.Framework.SignalR
{
    public class SignalRRealTimeNotifier:IRealTimeNotifier
    {
        private readonly IOnlineClientManager mOnlineClientManager;

        private static IHubContext<TwinkleHub> TwHub
        {
            get
            {
                return TwinkleContext.GetService<IHubContext<TwinkleHub>>();
            }
        }

        public SignalRRealTimeNotifier(IOnlineClientManager onlineClientManager)
        {
            mOnlineClientManager = onlineClientManager;
        }


        public Task SendNotificationsAsync(UserNotification[] userNotifications)
        {
            foreach (var userNotification in userNotifications)
            {
                try
                {
                    var onlineClients = mOnlineClientManager.GetAllByUserId(userNotification.UserId, userNotification.TenantId);
                    foreach (var onlineClient in onlineClients)
                    {
                        var signalRClient = TwHub.Clients.Client(onlineClient.ConnectionId);
                        if (signalRClient == null)
                        {
                            continue;
                        }

                        signalRClient.SendCoreAsync("getNotification",new object[] { userNotification.Data });
                    }
                }
                catch (Exception ex)
                {
                    //Logger.Warn("Could not send notification to user: " + userNotification.ToUserIdentifier());
                    //Logger.Warn(ex.ToString(), ex);
                }
            }

            return Task.FromResult(0);
        }
    }
}
