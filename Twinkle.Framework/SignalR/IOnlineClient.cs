namespace Twinkle.Framework.SignalR
{
    public interface IOnlineClient
    {
        string ConnectionId { get; set; }
        string UserId { get; set; }
        string TenantId { get; set; }
    }
}
