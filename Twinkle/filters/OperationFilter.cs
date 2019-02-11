using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Twinkle.Framework.Database;
using Twinkle.Framework.Extensions;

namespace Twinkle.filters
{
    /// <summary>
    /// 常规操作日志 记录所有api的调用情况
    /// </summary>
    public class OperationFilter : IActionFilter, IFilterMetadata
    {
        private const string keyDur = "__action_duration__";
        private const string keyStart = "__action_start__";
        private const string keyParam = "__action_parameters__";
        public void OnActionExecuting(ActionExecutingContext context)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            context.HttpContext.Items[keyDur] = stopWatch;
            context.HttpContext.Items[keyStart] = DateTime.Now;
            context.HttpContext.Items[keyParam] = context.ActionArguments;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var items = context.HttpContext.Items;
            long? duration = null;
            Stopwatch stopWatch = items[keyDur] as Stopwatch;
            stopWatch?.Stop();
            duration = stopWatch?.ElapsedMilliseconds;

            DateTime start = (DateTime)items[keyStart];

            string paramJson = string.Empty;

            IDictionary<string, object> parameters = items[keyParam] as IDictionary<string, object>;
            if (parameters != null)
            {
                if (parameters.Count == 1)
                {
                    ClientModel cm = parameters.FirstOrDefault().Value as ClientModel;
                    if (cm != null)
                    {
                        paramJson = cm.GetClientParams().ToString();
                    }
                    else
                    {
                        paramJson = JToken.FromObject(parameters).ToString();
                    }
                }
                else if (parameters.Count > 1)
                {
                    paramJson = JToken.FromObject(parameters).ToString();
                }
            }


            var user = TwinkleContext.User;

            DatabaseManager db = TwinkleContext.GetService<DatabaseManager>();
            db.ExecuteNonQuery(@"INSERT INTO OperationLogs
              (
                TenantId
               ,UserId
               ,Area
               ,Controller
               ,[Action]
               ,[Parameters]
               ,ExecutionTime
               ,ExecutionDuration
               ,ClientIpAddress
               ,ClientName
               ,BrowserInfo
               ,Exception
              )
            VALUES
              (
                @TenantId
               ,@UserId
               ,@Area
               ,@Controller
               ,@Action
               ,@Parameters
               ,@ExecutionTime
               ,@ExecutionDuration
               ,@ClientIpAddress
               ,@ClientName
               ,@BrowserInfo
               ,@Exception
              )", new
            {
                user?.TenantId,
                user?.UserId,
                Area = context.RouteData.Values["area"]?.ToString(),
                Controller = context.RouteData.Values["controller"]?.ToString(),
                Action = context.RouteData.Values["action"]?.ToString(),
                Parameters = paramJson,
                ExecutionTime = start,
                ExecutionDuration = duration,
                ClientIpAddress = context.HttpContext.Connection.RemoteIpAddress.ToString().Replace("::1", "127.0.0.1"),
                ClientName = "",
                BrowserInfo = context.HttpContext.Request.Headers["User-Agent"],
                Exception = context.Exception?.Message
            });
        }


    }
}
