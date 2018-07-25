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

namespace Twinkle.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        
        public IActionResult Index1(int id)
        {
            return Ok("ok");
        }

        public void Index2(ClientModel client)
        {
            string a = client.GetString("key1");
            int? b = client.GetInt("key2");
            DateTime? c = client.GetDateTime("key3");
            double? d = client.GetDouble("key4");

            ddd e = client.GetEntity<ddd>("ttdd:ddd");
        }

        public void Index3(UploadFileArgs upload)
        {
           
        }


        public JsonResult ReadCache()
        {
            return Json(new
            {
                Cache = TwinkleContext.Cache.Get("AABB")
            });
        }

        public JsonResult ReadSeesion()
        {
            return Json(new
            {
                Session = TwinkleContext.MvcHttpContext.Session.GetString("AABB")
            });
        }

        public void WriteToken(string token)
        {
            TwinkleContext.MvcHttpContext.Response.Headers["Access-Control-Expose-Headers"] = "access-token";
            TwinkleContext.MvcHttpContext.Response.Headers["access-token"] = token;
        }
    }

    public class ddd
    {
        public string name { get; set; }
        public int? age { get; set; }
        public DateTime? birthday { get; set; }
    }
}