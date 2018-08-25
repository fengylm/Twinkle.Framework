using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Twinkle.Framework.Security;

namespace Twinkle.Framework.Mvc
{
    public class TwinkleMiddleware
    {
        private readonly RequestDelegate _next;

        public TwinkleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            //if (!ValidJWT(context))
            //{
            //    context.Response.StatusCode = 401;
            //    return Task.CompletedTask;
            //}
            return _next(context);
        }

        private bool ValidJWT(HttpContext context)
        {
            string author = context.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(author))
            {
                if (author.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var jwt = TwinkleContext.GetService<JWT>();
                    return jwt.Valid(author.Substring(7));
                }
                return true;
            }
            return true;
        }
    }
}
