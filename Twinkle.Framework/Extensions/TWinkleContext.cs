using Twinkle.Framework.Cache;
using Twinkle.Framework.Security.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Twinkle.Framework.Extensions
{
    public class TwinkleContext
    {
        #region 全局对象注入
        internal static IServiceCollection ServiceCollection { get; set; }
        internal static IHttpContextAccessor HttpContextAccessor => ServiceCollection.BuildServiceProvider().GetService<IHttpContextAccessor>();
        internal static IServiceProvider ServiceProvider => HttpContextAccessor.HttpContext.RequestServices;
        internal static IHostingEnvironment HostingEnvironment => ServiceCollection.BuildServiceProvider().GetService<IHostingEnvironment>();
        #endregion

        #region 获取缓存服务
        public static ICacheService Cache => GetService<ICacheService>();
        public static ISession Session => HttpContext.Session;
        #endregion

        #region 获取配置服务
        public static IConfigurationRoot Config => GetService<IConfigurationRoot>();
        #endregion

        #region 获取注册服务
        /// <summary>
        /// 获取服务,获取不到返回null
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }
        /// <summary>
        ///  获取服务,获取不到返回null
        /// </summary>
        /// <param name="service">服务类型</param>
        /// <returns></returns>
        public static object GetService(Type service)
        {
            return ServiceProvider.GetService(service);
        }
        /// <summary>
        /// 获取服务,获取不到报异常
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns></returns>
        public static T GetRequiredService<T>()
        {
            return ServiceProvider.GetRequiredService<T>();
        }
        /// <summary>
        /// 获取服务,获取不到报异常
        /// </summary>
        /// <param name="service">服务类型</param>
        /// <returns></returns>
        public static object GetRequiredService(Type service)
        {
            return ServiceProvider.GetRequiredService(service);
        }
        #endregion

        #region 应用程序信息
        /// <summary>
        /// 应用程序根目录,appsettings.json所在目录
        /// </summary>
        public static string AppRoot => HostingEnvironment.ContentRootPath;

        /// <summary>
        /// 应用程序静态地址根目录,默认就是wwwroot目录,包含在AppRoot下
        /// </summary>
        public static string WWWRoot => HostingEnvironment.WebRootPath;

        /// <summary>
        /// 应用程序名称
        /// </summary>
        public static string AppName => HostingEnvironment.ApplicationName;

        /// <summary>
        /// 应用程序根Url
        /// </summary>
        public static string UrlRoot => $"{(string.IsNullOrEmpty(HttpContext.Request.Scheme) ? "" : HttpContext.Request.Scheme + "://")}{HttpContext.Request.Host}{HttpContext.Request.PathBase}";

        /// <summary>
        /// 获取Mvc的HttpContext对象
        /// </summary>
        public static HttpContext HttpContext => HttpContextAccessor.HttpContext;

        /// <summary>
        /// 获取完整请求路径
        /// </summary>
        public static string AbsoluteUri => $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}{HttpContext.Request.Path}{HttpContext.Request.QueryString}";
        #endregion

        #region 用户登陆验证
        #region 登录/登出
        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="User">用户对象</param>
        /// <param name="Expires">登陆信息保存时间(分钟) 默认取值为 配置文件的Authentication:Expires</param>
        /// <returns></returns>
        public static void Login(AuthUser User, int? Expires = null)
        {
            var jwt = GetService<TokenAuthManager>();
            string token = jwt.CreateToken(User, Expires);

            Session.SetString("AuthUser", JToken.FromObject(User).ToString());

            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "access-token";
            HttpContext.Response.Headers["access-token"] = token;
        }

        /// <summary>
        /// 系统登出
        /// </summary>
        /// <returns></returns>
        public static void Logout()
        {
            var jwt = GetService<TokenAuthManager>();
            Session.Clear();
            jwt.DestroyToken(UserToken);
        }
        #endregion
        #region 用户信息
        /// <summary>
        /// 获取已经登陆的用户数据
        /// </summary>
        public static object UserData => HttpContext.User.Claims.Where(c => c.Type == TwinkleClaimTypes.UserData).FirstOrDefault()?.Value;

        /// <summary>
        /// 获取用户token
        /// </summary>
        public static string UserToken
        {
            get
            {
                string author = HttpContext.Request.Headers["Authorization"];
                if (!string.IsNullOrEmpty(author))
                {
                    if (author.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        return author.Substring(7);
                    }
                    return "";
                }
                return "";
            }
        }
        #endregion
        #endregion
    }
}
