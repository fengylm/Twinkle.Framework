using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Twinkle.Framework.Extensions;
using Twinkle.Framework.Security.Authorization;
using Twinkle.Models;

namespace Twinkle.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        [AllowAnonymous]
        public IActionResult Index()
        {
            string apiPath = $"window.apiPath='{TwinkleContext.UrlRoot}';";
            string printService = $"window.printService='http://www.comstarsoft.com/PrintService/report.aspx';";

            ViewBag.Content = System.IO.File.ReadAllText(Path.Combine(TwinkleContext.WWWRoot, "index.html"))
                .Replace("/css/", "css/")
                .Replace("/js/", "js/")
                .Replace("// config injected", apiPath + printService);

            return View();

        }

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
                        cUrl = "/TRender",
                        cPath = "demo/TRender",
                        cTitle = "TRender",
                        cIcon = "table"
                    },
                     new Router
                    {
                        cUrl = "/DesignPage",
                        cPath = "demo/DesignPage",
                        cTitle = "DesignPage",
                        cIcon = "table"
                    },
                    new Router
                    {
                        cUrl = "/table",
                        cPath = "demo/ttable",
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
            TwinkleContext.Login(new AuthUser { UserId = "admin" }, 200);
            return Json(new { status = 0 });
        }

        public JsonResult Logout()
        {
            TwinkleContext.Logout();
            return Json(new { status = 0 });
        }

        public JsonResult GetData(ClientModel client)
        {
            int? start = client.GetInt("_start");
            int? limit = client.GetInt("_limit");
            int? id = client.GetInt("id");

            List<object> lst = new List<object>();
            lst.Add(new
            {
                datetime = DateTime.Now.ToString(),
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                time = DateTime.Now.ToString(),
                number = 19.22,
                select = "1",
                @string = "上海市普陀区金沙江路 1518 弄"
            });
            lst.Add(new
            {
                datetime = DateTime.Now.ToString(),
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                time = DateTime.Now.ToString(),
                number = 12.01,
                select = "0",
                @string = "上海市普陀区金沙江路 1518 弄"
            });

            return Json(new { total = 2, data = lst });
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