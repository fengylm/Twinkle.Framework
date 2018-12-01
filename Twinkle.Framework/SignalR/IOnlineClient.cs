using System;
using System.Collections.Generic;
using System.Text;

namespace Twinkle.Framework.SignalR
{
    public interface IOnlineClient
    {
        string ConnectionId { get; set; }
        string UserId { get; set; }
        int? TenantId { get; set; }
    }
}
