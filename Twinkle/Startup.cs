using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using Twinkle.filters;
using Twinkle.Framework.Extensions;

namespace Twinkle
{
    public class Startup
    {
        //public static ILoggerRepository repository { get; set; }
        //public Startup(IConfiguration configuration, IHostingEnvironment env)
        //{
        //    //log4net
        //    repository = LogManager.CreateRepository("LogRepository");
        //    //指定配置文件
        //    XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));

        //}

        public void ConfigureServices(IServiceCollection services)
        {
            #region 添加配置信息服务
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            services.AddSingleton(typeof(IConfigurationRoot), config);
            #endregion

            #region 添加全局异常处理
            List<IFilterMetadata> lstFilter = new List<IFilterMetadata>();
            lstFilter.Add(new OperationFilter());
            lstFilter.Add(new ExceptionFilter());
            #endregion

            services.AddTwinkle(config, lstFilter);

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfigurationRoot config)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler("/Home/Error");

            app.UseTwinkle(config, routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}",
                    defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                       name: "areas",
                       template: "{area}/{controller}/{action}"
                     );

                routes.MapSpaFallbackRoute(
                   name: "spa-fallback",
                   defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
