using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Twinkle.Framework.Database;
using Twinkle.Framework.Extensions;

namespace Twinkle.filters
{
    public class ExceptionFilter : IExceptionFilter, IFilterMetadata
    {
        public void OnException(ExceptionContext context)
        {
            var logger = TwinkleContext.GetService<ILogger<ExceptionFilter>>();
            DatabaseManager Db = TwinkleContext.GetRequiredService<DatabaseManager>();

            if (context.ExceptionHandled == false)
            {
                string msg = context.Exception.Message;

                if (Db.Transaction != null)
                {
                    Db.Rollback();
                }

                logger.LogError($"{string.Join("/", context.RouteData.Values.Values)} - {msg}");

                context.Result = new JsonResult(new { status = 1, msg })
                {
                    StatusCode = StatusCodes.Status200OK,
                    ContentType = "text/html;charset=utf-8"
                };
            }
            context.ExceptionHandled = true; // 标记已经被处理 不再冒泡
        }
    }
}
