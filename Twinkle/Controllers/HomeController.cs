using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using Twinkle.Framework.Extensions;
using Twinkle.Framework.Security.Authorization;
using Twinkle.Framework.SignalR;
using Twinkle.Models;

namespace Twinkle.Controllers
{
    public class HomeController : BaseController
    {

        [AllowAnonymous]
        public IActionResult Index()
        {
            string baseUrl = $"window.baseUrl='{TwinkleContext.UrlRoot}';";
            string reportUrl = $"window.reportUrl='http://www.comstarsoft.com/PrintService/report.aspx';";

            ViewBag.Content = System.IO.File.ReadAllText(Path.Combine(TwinkleContext.WWWRoot, "index.html"))
                .Replace("/static", "static")
                .Replace("// config injected", baseUrl + reportUrl);

            return View();

        }

        [AllowAnonymous]
        public JsonResult Login()
        {
            int? tenantId = null;
            TwinkleContext.Login(new AuthUser { UserId = "admin" }, 200);
            return Json(new
            {
                status = 0,
                userId = "admin",
                tenantId,
            });
        }

        [AllowAnonymous]
        public JsonResult Logout()
        {
            TwinkleContext.Logout();
            return Json(new { status = 0 });
        }

        [AllowAnonymous]
        public void SendNotify()
        {
            IRealTimeNotifier rtf = TwinkleContext.GetService<IRealTimeNotifier>();

            rtf.SendNotificationsAsync(new UserNotification[] {
                new UserNotification{ UserId="admin",Data=new NotifyData{ Channel="test",Data=new { key="key",num=2 }  } }
            });
        }

        public JsonResult GetRouters()
        {
            List<Router> lstRouter = new List<Router>();
            lstRouter.Add(new Router
            {
                Url = "/setting",
                Title = "设置",
                Icon = "table",
                Children = new Router[] {
                    new Router
                    {
                        Url = "/user",
                        Path = "base/User",
                        Title = "用户信息",
                        Icon = "table"
                    },
                }
            });
            lstRouter.Add(new Router
            {
                Url = "/Demo",
                Title = "Demo",
                Icon = "table",
                Children = new Router[] {
                    new Router
                    {
                        Url = "/Examples",
                        Path = "demo/Examples",
                        Title = "Examples",
                        Icon = "table"
                    },
                }
            });
            lstRouter.Add(new Router
            {
                Url = "/Examples1",
                Title = "(Examples)独立页面",
                Path = "demo/Examples",
                Icon = "table",
                IsSingle = true,
            });
            lstRouter.Add(new Router
            {
                Url = "/Test",
                Title = "(Test)内置页面",
                Path = "demo/Test",
                Icon = "table",
                IsSingle = false,
            });

            return Json(lstRouter);
        }


        [AllowAnonymous]
        public void Error()
        {
            // 将未捕捉到的异常信息写入到响应体中,此操作具有安全隐患 在稳定上线之后需要移除
            Exception ex = this.HttpContext.Features.Get<IExceptionHandlerPathFeature>().Error;

            this.HttpContext.Response.ContentType = "text/plain;charset=utf-8";
            this.HttpContext.Response.WriteAsync(Environment.NewLine);
            this.HttpContext.Response.WriteAsync($"Message:{ex.Message}");
            this.HttpContext.Response.WriteAsync(Environment.NewLine);
            this.HttpContext.Response.WriteAsync($"Type:{ex.GetType().FullName}");
            this.HttpContext.Response.WriteAsync(Environment.NewLine);
            this.HttpContext.Response.WriteAsync($"StackTrace:{ex.StackTrace}");
            this.HttpContext.Response.WriteAsync(Environment.NewLine);

        }
    }

}