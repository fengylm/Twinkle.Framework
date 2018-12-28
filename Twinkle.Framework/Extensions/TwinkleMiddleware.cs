using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Twinkle.Framework.Extensions
{
    public class TwinkleMiddleware
    {
        private readonly RequestDelegate _next;

        public TwinkleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            //string style = "<style>* {margin: 0;padding: 0;}body, html {height: 100%;width: 100% }.remask {top: 0;right: 0;bottom: 0;left: 0;position: absolute;opacity: 0.3;background-color: black;z-index: 99998;}.window_boundingBox {background-color: #fff;border: 1px solid #4d99cb;border-radius: 10px;box-shadow: 0 0 5px rgba(0, 0, 0, 0.3);overflow: hidden;position: absolute;left: 50%;top: 50%;transform: translate(-50%, -50%);height: 150px;width: 400px;z-index: 99999;}.window_header {color: #fff;width: 100%;background-color: #71aacf;height: 40px;line-height: 40px;padding-left: 20px;}.window_body {padding: 10px;}.closeBtn {border-radius: 50%;cursor: pointer;width: 20px;height: 20px;padding: 2px;text-align: center;line-height: 20px;background-color: #fff;color: #333;position: absolute;right: 6px;top: 6px;color: #4d99cb;}.button {-webkit-border-radius: 8;-moz-border-radius: 8;border-radius: 8px;text-shadow: 0px 1px 0px #3d768a;-webkit-box-shadow: inset 0px 10px 14px -7px #276873;-moz-box-shadow: inset 0px 10px 14px -7px #276873;box-shadow: inset 0px 10px 14px -7px #276873;font-family: Arial;color: #ffffff;font-size: 16px;background: #3498db;padding: 6px 30px 6px 30px;border: solid #1f628d 1px;text-decoration: none;}.button:hover {color: #ffffff;background: #408c99;text-decoration: none;}.registInput {resize: none;border-style: outset;width: 100%;height: 150px;}</style>";

            //string title = "应用许可证还有N天到期";

            //string close = " <div class=\"closeBtn\" onclick=\"javascript: document.getElementById('registBox').remove(); document.getElementById('registMask').remove(); \">X</div> ";

            //string dialog = "<form action=\"/eltwinkle/Home\" method=\"post\" id=\"registForm\"><div id=\"registMask\" class=\"remask\"></div> <div id=\"registBox\" class=\"window_boundingBox\"> <div class=\"window_header\">服务到期提醒</div> <div class=\"window_body\"> {0}</div>{1}</div></form>";

            //await context.Response.WriteAsync(style + string.Format(dialog, title, close));
        }
    }
}
