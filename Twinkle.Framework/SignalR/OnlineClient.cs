namespace Twinkle.Framework.SignalR
{
    public class OnlineClient: IOnlineClient
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }
    }
}
