using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Twinkle.Framework.SignalR
{
    public interface IRealTimeNotifier
    {
        Task SendNotificationsAsync(UserNotification[] userNotifications);
    }
}
