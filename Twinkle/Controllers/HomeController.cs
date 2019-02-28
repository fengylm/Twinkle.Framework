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
using System.Linq;

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
            List<Sys_Module> listModule = Db.ExecuteEntities<Sys_Module>(@"select * from Sys_Module where  cCode in ( select cModuleCode from Sys_UserInRole T 
                                                                        left join Sys_RoleForModule T1 on T1.nRoleID = T.nRoleID and T1.TenantId = T.TenantId
                                                                        where T.TenantId = @TenantId and  T.UserId = @UserId ) order by nOrderID ", new { TenantId = Auth.TenantId, UserId = Auth.UserId });

            var toplevel= listModule.Where(p => p.nPID == 0).ToList();
            List<Router> lstRouter = new List<Router>();

            foreach (var item in toplevel)
            {
                Router router = new Router();
                router.Title = item.cTitle;
                router.Url = item.cRoute;
                router.Path = item.cPath;
                router.Icon = item.cIcon;
                if (router.Children == null)
                {
                    router.Children = new List<Router>();
                }
                List<Sys_Module> list = listModule.Where(p => p.nPID == item.ID).ToList();

                GetRouterChildren(router,list);
                
                lstRouter.Add(router);
            }

            return Json(lstRouter);
        }
        public void GetRouterChildren(Router router,List<Sys_Module> list)
        {
            foreach (var item in list)
            {
                Router subRouter = new Router();
                subRouter.Title = item.cTitle;
                subRouter.Url = item.cRoute;
                subRouter.Path = item.cPath;
                subRouter.Icon = item.cIcon;
                if (subRouter.Children == null)
                {
                    subRouter.Children = new List<Router>();
                }
                router.Children.Add(subRouter);
            }
        }

        /// <summary>
        /// 获取列信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public JsonResult GetColumnConfig(string code)
        {
            string strSQL = @"SELECT * FROM Sys_ColumnsForModule T
                                INNER JOIN Sys_RoleForColumn T1 ON T.cModuleCode=T1.cModuleCode AND T.cField=T1.cField
                                INNER JOIN Sys_UserInRole T2 ON T1.nRoleID=T2.nRoleID AND T1.TenantId=T2.TenantId
                                WHERE T.iShow=1 AND T.cModuleCode=@code and  T2.TenantId=@TenantId AND T2.UserId=@UserId
                            ORDER BY T.nOrderID";
            return Json(new { status = 0, data = Db.ExecuteEntities<Sys_ColumnsForModule>(strSQL, new { code,Auth.TenantId,Auth.UserId }) });
        }

        /// <summary>
        /// 加载按钮权限
        /// </summary>
        /// <param name="cModuleCode"></param>
        /// <returns></returns>
        public JsonResult LoadButtonRole(string code)
        {
            string strSQL = @"SELECT distinct T.cButtonID FROM Sys_ButtonsForModule T
                                INNER JOIN Sys_RoleForButton T1 ON T.cModuleCode=T1.cModuleCode and T.cButtonID=T1.cButtonID
                                INNER JOIN Sys_UserInRole T2 ON T1.nRoleID=T2.nRoleID AND T1.TenantId=T2.TenantId 
                                WHERE T.cModuleCode=@code AND T2.TenantId=@TenantId AND T2.UserId=@UserId";

            return Json(new { status = 0, data = Db.ExecuteEntities<dynamic>(strSQL, new { code, Auth.TenantId, Auth.UserId }) });

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