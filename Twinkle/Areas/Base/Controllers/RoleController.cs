using Microsoft.AspNetCore.Mvc;
using Twinkle.Controllers;
using Twinkle.Framework.Extensions;

namespace Twinkle.Areas.Base.Controllers
{
    public class RoleController : BaseController
    {
        public JsonResult GetList(ClientModel client)
        {
            return this.Paging("SELECT * FROM Sys_Role", "ID", client);
        }
    }
}