using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Twinkle.Framework.Security.Authorization
{
    public static class AuthConfigurer
    {
        public static void Configure(IServiceCollection services, IConfigurationRoot config)
        {
            if (config.GetValue<bool>("Authorization:Enable"))
            {
                services.AddAuthentication(options =>
                {
                    //认证middleware配置
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    //主要是Authentication参数设置
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "Twinkle",

                        ValidateAudience = true,
                        ValidAudience = "TwinkleClient",

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("Authorization:SecurityKey"))),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                    o.SecurityTokenValidators.Clear();
                    o.SecurityTokenValidators.Add(new TokenValidator());
                    o.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = MessageReceived
                    };
                });

                services.AddSingleton(typeof(TokenAuthManager), (_) => { return new TokenAuthManager(config.GetValue<string>("Authorization:SecurityKey"), config.GetValue<int>("Authorization:Expires")); });
            }
        }

        private static Task MessageReceived(MessageReceivedContext context)
        {
            //if (!context.HttpContext.Request.Path.HasValue || !context.HttpContext.Request.Path.Value.StartsWith("/signalr"))
            //{
            //    return Task.CompletedTask;
            //}


            var qsAuthToken = context.Request.Query["accessToken"].ToString();
            if (qsAuthToken == null)
            {
                return Task.CompletedTask;
            }

            context.Token = qsAuthToken;
            return Task.CompletedTask;
        }
    }
}
