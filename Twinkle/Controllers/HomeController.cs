using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Twinkle.Framework.Cache;
using Twinkle.Framework.Mvc;
using Twinkle.Framework.Security;
using Twinkle.Models;

namespace Twinkle.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public JsonResult GetRouters()
        {
            List<Router> lstRouter = new List<Router>();
            lstRouter.Add(new Router
            {
                cUrl = "/example",
                cTitle = "Example1",
                cIcon = "table",
                Children = new Router[] {
                    new Router
                    {
                        cUrl = "/tree",
                        cPath = "layout/Login",
                        cTitle = "Tree",
                        cIcon = "table"
                    },
                    new Router
                    {
                        cUrl = "/table",
                        cPath = "layout/Login",
                        cTitle = "Table",
                        cIcon = "table"
                    }
                }
            });
            lstRouter.Add(new Router
            {
                cUrl = "/example1",
                cTitle = "Example2",
                cPath = "layout/Login",
                cIcon = "table"
            });

            return Json(lstRouter);
        }

        [AllowAnonymous]
        public JsonResult Login()
        {
            TwinkleContext.Login(new { uid = "admin", userName = "系统管理员" },2);
            return Json(new { status = 0 });
        }

        public JsonResult Logout()
        {
            TwinkleContext.Logout();
            return Json(new { status = 0 });
        }
    }
}