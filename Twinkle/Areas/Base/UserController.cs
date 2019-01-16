using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Twinkle.Controllers;
using Twinkle.Framework.Extensions;

namespace Twinkle.Areas.Base
{
    public class UserController : BaseController
    {
        public JsonResult GetList(ClientModel client)
        {
            return this.Paging("SELECT * FROM Sys_User", "ID", client);
        }
    }
}