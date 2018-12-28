using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
            string apiPath = $"window.apiPath='{TwinkleContext.UrlRoot}';";
            string printService = $"window.printService='http://www.comstarsoft.com/PrintService/report.aspx';";

            ViewBag.Content = System.IO.File.ReadAllText(Path.Combine(TwinkleContext.WWWRoot, "index.html"))
                .Replace("/static", "static")
                .Replace("// config injected", apiPath + printService);

            return View();

        }

        public JsonResult GetRouters()
        {
            List<Router> lstRouter = new List<Router>();
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
                Url = "/Examples2",
                Title = "(Examples)内置页面",
                Path = "demo/Examples",
                Icon = "table",
                IsSingle = false,
            });

            return Json(lstRouter);
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

        public JsonResult GetData(ClientModel client)
        {
            return this.Paging("SELECT * FROM TWTABLE", "id", client);
        }

        [AllowAnonymous]
        public JsonResult GetHead(ClientModel client)
        {

            dynamic obj = new[] {
                new {
                     label= "数据测试",
                     align= "center",
                     columns=new object[]{
                          new {
                              prop= "datetime",
                              label= "日期时间",
                              width= 180,
                              attrs=
                                new {
                                    type= "datetime",
                                editable= true
                              }
                            },
                            new {
                              prop= "date",
                              label= "日期",
                              width= 180,
                              attrs= new {
                                type= "date",
                                editable= true
                              }
                             },
                            new {
                              prop= "datetime",
                              label= "时间",
                              width= 180,
                              attrs= new {
                                type= "time",
                                editable= true
                              }
                            },
                            new {
                              prop= "number",
                              label= "数字",
                              width= 180,
                              attrs= new {
                                type= "number",
                                precision= 2,
                                min= -10,
                                max= 20,
                                editable= true
                              }
                            },
                            new {
                              prop= "select",
                              label= "下拉",
                              width= 180,
                              attrs= new {
                                type= "select",
                                editable= true,
                                options= new JRaw("function options() {return _this.options;}"),
                                value= "value",
                                label= "label"
                              }
                            },
                            new {
                              prop= "select",
                              label= "布尔",
                              width= 180,
                              attrs= new {
                                type= "boolean",
                                editable= true
                              }
                            },
                            new {
                              prop= "string",
                              label= "文本",
                              attrs= new {
                                type= "string",
                                maxlength= 20,
                                minlength= 3,
                                editable= true
                              }
                            }
                     }
                }
            };
            return Json(obj);
        }


    }

    public class JavaScriptSettings
    {
        public Newtonsoft.Json.Linq.JRaw OnLoadFunction { get; set; }
        public Newtonsoft.Json.Linq.JRaw OnSucceedFunction { get; set; }
    }

}