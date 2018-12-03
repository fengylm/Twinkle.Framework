namespace Twinkle.Framework.SignalR
{
    public class UserNotification
    {
        public string UserId { get; set; }
        public int? TenantId { get; set; }
        public NotifyData Data { get; set; }
    }
}
