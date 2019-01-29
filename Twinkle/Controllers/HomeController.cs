using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using Twinkle.Framework.Extensions;
using Twinkle.Framework.Security.Authorization;
using Twinkle.Framework.Security.Cryptography;
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
        public JsonResult Login(ClientModel client)
        {
            string UserId = client.GetString("UserId");
            string Password = client.GetString("Password");

            string tenantId = this.Auth.TenantId;// 后期 tenantId 一般会是从登陆界面带过来

            Sys_User user = Db.ExecuteEntity<Sys_User>("SELECT * FROM Sys_User WHERE UserId=@UserId and TenantId=@TenantId", new { UserId, TenantId = tenantId });

            if (user == null)
            {
                return Json(new
                {
                    status = 1,
                    msg = "账号不存在."
                });
            }

            user.cLoginIP = this.Request.HttpContext.Connection.RemoteIpAddress.ToString().Replace("::1", "127.0.0.1");


            if (user.cPassword != DataCipher.MD5Encrypt(user.UserId + user.cNonceStr + Password))
            {

                if (user.dUnlockDate > DateTime.Now)
                {
                    return Json(new
                    {
                        status = 1,
                        msg = "账户已经被锁定,请稍后再试或联系管理员."
                    });
                }

                if ((DateTime.Now - (user.dLoginDate ?? DateTime.MinValue)).TotalMinutes > 30)
                {
                    user.nFailedCount = 0;
                }

                user.nFailedCount = (user.nFailedCount ?? 0) + 1;

                if (user.nFailedCount == 5)
                {
                    user.dUnlockDate = DateTime.Now.AddMinutes(20);
                }
                else
                {
                    user.dUnlockDate = null;
                }

                Db.ExecuteNonQuery("UPDATE Sys_User SET cLoginIP = @cLoginIP,dLoginDate = GETDATE(),nFailedCount =@nFailedCount,dUnlockDate=@dUnlockDate WHERE UserId=@UserId AND TenantId=@TenantId", user);

                if (user.nFailedCount == 5)
                {
                    return Json(new
                    {
                        status = 1,
                        msg = "由于多次密码错误,账号已经被锁定,请20分钟后重试."
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = 1,
                        msg = $"账号或密码错误,无法登陆,还可尝试 {5 - user.nFailedCount} 次."
                    });
                }
            }
            else
            {
                if (user.dUnlockDate > DateTime.Now)
                {
                    return Json(new
                    {
                        status = 1,
                        msg = "账户已经被锁定,请稍后再试或联系管理员."
                    });
                }

                if (user.iStatus == 0)
                {
                    return Json(new
                    {
                        status = 1,
                        msg = "账户已经被停用,请联系管理员."
                    });
                }

                user.nFailedCount = 0;
                user.dUnlockDate = null;

                Db.ExecuteNonQuery("UPDATE Sys_User SET cLoginIP = @cLoginIP,dLoginDate = GETDATE(),nFailedCount =@nFailedCount WHERE UserId=@UserId AND TenantId=@TenantId", user);

                TwinkleContext.Login(new AuthUser { UserId = UserId, TenantId = tenantId });
                return Json(new
                {
                    status = 0,
                    userId = UserId,
                    tenantId,
                });
            }




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
                Children = new Router[] {
                     new Router
                    {
                        Url = "/module",
                        Path = "base/Module",
                        Title = "功能模块",
                    },
                    new Router
                    {
                        Url = "/user",
                        Path = "base/User",
                        Title = "用户信息",
                    },
                    new Router
                    {
                        Url = "/role",
                        Path = "base/Role",
                        Title = "权限信息",
                    },
                }
            });
            lstRouter.Add(new Router
            {
                Url = "/Demo",
                Title = "Demo",
                Children = new Router[] {
                    new Router
                    {
                        Url = "/Examples",
                        Path = "demo/Examples",
                        Title = "Examples",
                    },
                }
            });
            lstRouter.Add(new Router
            {
                Url = "/Examples1",
                Title = "(Examples)独立页面",
                Path = "demo/Examples",
                IsSingle = true,
            });
            lstRouter.Add(new Router
            {
                Url = "/Test",
                Title = "(Test)内置页面",
                Path = "demo/Test",
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