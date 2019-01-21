using Twinkle.Framework.Cache;
using Twinkle.Framework.Security.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Twinkle.Framework.SignalR;
using Twinkle.Framework.Database;

namespace Twinkle.Framework.Extensions
{
    public static class TwinkleServiceCollectionExtensions
    {
        public static IServiceCollection AddTwinkle(this IServiceCollection services, IConfigurationRoot config, IList<IFilterMetadata> filters = null)
        {

            TwinkleContext.ServiceCollection = services;

            #region 添加http上下文服务
            services.AddHttpContextAccessor();
            #endregion

            #region 添加缓存服务
            switch (config.GetValue<string>("CacheStrategy"))
            {
                case "Redis":
                    services.AddDistributedRedisCache(options =>
                    {
                        options.Configuration = config.GetValue<string>("Redis:ServerHosts").Split(',', StringSplitOptions.RemoveEmptyEntries)[0];
                        options.InstanceName = config.GetValue<string>("Redis:ServerName");
                    });

                    services.AddSingleton(typeof(RedisService), (_) =>
                    {
                        return new RedisService(new RedisConfig
                        {
                            Password = config.GetValue<string>("Redis:Password"),
                            SentinelHosts = config.GetValue<string>("Redis:SentinelHosts").Split(',', StringSplitOptions.RemoveEmptyEntries),
                            ServerHosts = config.GetValue<string>("Redis:ServerHosts").Split(',', StringSplitOptions.RemoveEmptyEntries),
                            ServerName = config.GetValue<string>("Redis:ServerName")
                        });
                    });
                    break;
                default:
                    services.AddDistributedMemoryCache();
                    break;
            }
            services.AddSingleton(typeof(ICacheService), typeof(CacheService));
            #endregion

            #region 添加身份认证服务
            AuthConfigurer.Configure(services, config);
            #endregion

            #region 添加Mvc服务
            services.AddMvc(options =>
            {
                //加载自定义Model绑定
                options.ModelBinderProviders.Insert(0, new TwinkleModelBinderProvider());
                //加载拦截器
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        options.Filters.Add(filter);
                    }
                }

            }).AddJsonOptions(options =>
            {   //序列化json格式时候,大写的首字母不自动转为小写
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            });
            #endregion

            #region 添加Session服务
            services.AddSession();
            #endregion

            #region 添加跨域支持服务
            if (config.GetValue<bool>("Cors:Enable"))
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("any", builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();//指定处理cookie,客户端跨域访问时 需要 设置 xhrFields: {withCredentials: true}
                    });
                });
            }
            #endregion

            #region 添加SignalR服务
            services.AddSingleton(typeof(IOnlineClientManager), typeof(OnlineClientManager));
            services.AddSingleton(typeof(IRealTimeNotifier), typeof(SignalRRealTimeNotifier));

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            }).AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            });
            #endregion

            #region 取消Form文件上传大小限制
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
            });
            #endregion

            #region 设置字符集
            //设置字符集
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            #endregion

            #region 添加数据库操作
            services.AddScoped<DatabaseManager>(); 
            #endregion

            return services;
        }

        /// <summary>
        /// 添加Twinkle支持
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configureRoutes">MVC路由配置</param>
        /// <param name="configureHub">Signalr代理配置</param>
        /// <returns></returns>
        public static IApplicationBuilder UseTwinkle(this IApplicationBuilder app, IConfigurationRoot config, Action<IRouteBuilder> configureRoutes = null)
        {
            #region 启用跨域
            // 允许跨域 放在所有需要跨域的use的最上面,否则就是一个老大的坑
            if (config.GetValue<bool>("Cors:Enable"))
            {
                app.UseCors("any");
            }
            #endregion

            #region 启用身份认证
            if (config.GetValue<bool>("Authorization:Enable"))
            {
                app.UseAuthentication();
            }
            #endregion

            #region 启用静态文件目录 默认是wwwroot
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true
            });
            #endregion

            #region 启用Session
            app.UseSession();
            #endregion

            #region SignalR代理配置
            app.UseSignalR(route =>
            {
                route.MapHub<TwinkleHub>("/signalr");
            });

            #endregion

            #region 启用中间件
            app.UseMiddleware<TwinkleMiddleware>();
            #endregion

            #region 路由配置
            app.UseMvc(configureRoutes);
            #endregion

            return app;
        }
    }
}
