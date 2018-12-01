using System;
using System.Collections.Generic;
using System.Text;

namespace Twinkle.Framework.Authorization
{
    public class User
    {
        public int? TenantId { get; set; }

        public string UserId { get; set; }

        public string GroupId { get; set; }

        public object UserData { get; set; }
    }
}
