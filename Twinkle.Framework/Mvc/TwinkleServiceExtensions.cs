using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Twinkle.Framework.Authorization;
using Twinkle.Framework.Cache;
using Twinkle.Framework.SignalR;

namespace Twinkle.Framework.Mvc
{
    public static class TwinkleServiceExtensions
    {
        /// <summary>
        /// 加载Twinkle服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="filters">自定义拦截器</param>
        /// <returns></returns>
        public static IServiceCollection AddTwinkle(this IServiceCollection services, IList<IFilterMetadata> filters = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            #region 添加http上下文服务
            services.AddHttpContextAccessor();
            TwinkleContext.ServiceCollection = services;
            #endregion

            #region 添加配置信息服务
            IConfigurationRoot Config = new ConfigurationBuilder().SetBasePath(TwinkleContext.AppRoot).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
            services.AddSingleton(typeof(AppConfig), (_) => { return new AppConfig(Config); });
            #endregion

            #region 添加缓存服务
            switch (TwinkleContext.Config.GetValue<string>("CacheStrategy"))
            {
                case "Redis":
                    services.AddDistributedRedisCache(options =>
                    {
                        options.Configuration = TwinkleContext.Config.GetValue<string>("Redis:ServerHosts").Split(',', StringSplitOptions.RemoveEmptyEntries)[0];
                        options.InstanceName = TwinkleContext.Config.GetValue<string>("Redis:ServerName");
                    });

                    services.AddSingleton(typeof(RedisService), (_) =>
                    {
                        return new RedisService(new RedisConfig
                        {
                            Password = TwinkleContext.Config.GetValue<string>("Redis:Password"),
                            SentinelHosts = TwinkleContext.Config.GetValue<string>("Redis:SentinelHosts").Split(',', StringSplitOptions.RemoveEmptyEntries),
                            ServerHosts = TwinkleContext.Config.GetValue<string>("Redis:ServerHosts").Split(',', StringSplitOptions.RemoveEmptyEntries),
                            ServerName = TwinkleContext.Config.GetValue<string>("Redis:ServerName")
                        });
                    });
                    break;
                default:
                    services.AddDistributedMemoryCache();
                    break;
            }
            services.AddSingleton(typeof(ICacheService), typeof(CacheService));

            #endregion

            #region 添加数据签名保护服务

            //services.AddDataProtection(opts =>
            //{
            //    opts.ApplicationDiscriminator = TwinkleContext.Config.GetValue<string>("AppID");
            //}).PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(TwinkleContext.AppRoot, $"{TwinkleContext.Config.GetValue<string>("AppID")}_keys")));

            #endregion

            #region 添加身份认证服务
            AuthConfigurer.Configure(services);
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

            }).AddJsonOptions(op =>
            {   //序列化json格式时候,大写的首字母不自动转为小写
                op.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                op.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            });
            #endregion

            #region 添加Session服务
            services.AddSession();
            #endregion

            #region 添加跨域支持服务
            if (TwinkleContext.Config.GetValue<bool>("EnableCors"))
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
            services.AddSignalR(op =>
                {
                    op.EnableDetailedErrors = true;
                }).AddJsonProtocol(op =>
                {
                    op.PayloadSerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                }); ;
            #endregion

            #region 取消Form文件上传大小限制
            services.Configure<FormOptions>(config =>
            {
                config.ValueLengthLimit = int.MaxValue;
                config.MultipartBodyLengthLimit = int.MaxValue;
            });
            #endregion

            #region 设置字符集
            //设置字符集
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
        public static IApplicationBuilder UseTwinkle(this IApplicationBuilder app, Action<IRouteBuilder> configureRoutes = null, IList<Action<HubRouteBuilder>> configureHubs = null)
        {
            #region 启用跨域
            // 允许跨域 放在所有需要跨域的use的最上面,否则就是一个老大的坑
            if (TwinkleContext.Config.GetValue<bool>("EnableCors"))
            {
                app.UseCors("any");
            }
            #endregion

            #region 启用身份认证
            if (TwinkleContext.Config.GetValue<bool>("Authentication:Enable"))
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

            if (configureHubs?.Count > 0)
            {
                foreach (var hubAction in configureHubs)
                {
                    app.UseSignalR(hubAction);
                }
            }
            app.UseSignalR(route =>
            {
                route.MapHub<SRHub>("/signalr");
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
