namespace Twinkle.Framework.Security.Authorization
{
    public class AuthUser
    {
        public string TenantId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string GroupId { get; set; }

        public object UserData { get; set; }
    }
}
