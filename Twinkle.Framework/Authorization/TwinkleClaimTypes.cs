using System;
using System.Collections.Generic;
using System.Text;

namespace Twinkle.Framework.Authorization
{
    public static class TwinkleClaimTypes
    {
        public const string Expiration = "http://schemas.microsoft.com/ws/2008/06/identity/claims/expiration";
        public const string Expired = "http://schemas.microsoft.com/ws/2008/06/identity/claims/expired";
        public const string UserData = "http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata";
        public const string TenantId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/TenantId";
        public const string UserId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/UserId";
        public const string GroupId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/GroupId";
        public const string IsActive = "http://schemas.microsoft.com/ws/2008/06/identity/claims/IsActive";
    }
}
