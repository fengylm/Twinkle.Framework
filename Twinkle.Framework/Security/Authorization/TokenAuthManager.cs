﻿using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Twinkle.Framework.Extensions;

namespace Twinkle.Framework.Security.Authorization
{
    public class TokenAuthManager
    {
        private string securityKey;
        private int expires;
        internal TokenAuthManager(string SecurityKey, int Expires)
        {
            securityKey = SecurityKey;
            expires = Expires;
        }

        /// <summary>
        /// 创建Token对象
        /// </summary>
        /// <param name="UserData">自定义包含用户信息的JSON对象,必须包含小写的uid节点</param>
        /// <param name="Expires">自定义超时时间(分钟),不设置默认获取配置文件中的超时时间</param>
        /// <returns></returns>
        public string CreateToken(AuthUser User, int? Expires = null)
        {
            if (User == null)
            {
                throw new ArgumentNullException("User不可以为null");
            }
            JToken userData = JToken.FromObject(User.UserData ?? "{}");

            Claim[] claim = new Claim[]{
                    new Claim(TwinkleClaimTypes.UserData, userData.ToString()),
                    new Claim(TwinkleClaimTypes.UserId, User.UserId),
                    new Claim(TwinkleClaimTypes.TenantId, User.TenantId?.ToString()??""),
                    new Claim(TwinkleClaimTypes.GroupId, User.GroupId?.ToString()??""),
                    new Claim(TwinkleClaimTypes.Expired,DateTime.Now.AddMinutes(Expires ?? expires).ToString("yyyy-MM-dd HH:mm:ss"))
                };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                    "Twinkle",
                    "TwinkleClient",
                    claim,
                    now,
                    now.AddMinutes(Expires ?? expires),
                    creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 获取UserData的json对象
        /// </summary>
        /// <param name="token">token字符串</param>
        public AuthUser GetUser(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                Claim[] Claims = tokenHandler.ReadJwtToken(token).Claims?.ToArray();
                if (Claims == null || Claims.Length == 0)
                {
                    return null;
                }
                else
                {
                    AuthUser user = new AuthUser();
                    foreach (Claim claim in Claims)
                    {
                        switch (claim.Type)
                        {
                            case TwinkleClaimTypes.UserId:
                                user.UserId = claim.Value;
                                continue;
                            case TwinkleClaimTypes.TenantId:
                                user.TenantId = claim.Value;
                                continue;
                            case TwinkleClaimTypes.GroupId:
                                user.GroupId = claim.Value;
                                continue;
                            case TwinkleClaimTypes.UserData:
                                if (!string.IsNullOrEmpty(claim.Value))
                                {
                                    user.UserData = JToken.Parse(claim.Value);
                                }
                                continue;
                        }
                    }
                    return user;
                }

            }
            catch
            {
                return null;
            }
        }

        ///// <summary>
        ///// 获取UserData的实例对象
        ///// </summary>
        ///// <typeparam name="T">要转换成的对象</typeparam>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //public T GetUserData<T>(string token)
        //{
        //    return JToken.Parse(GetUserData(token)).ToObject<T>();
        //}

        /// <summary>
        /// 获取token到期时间
        /// </summary>
        /// <param name="token">token</param>
        /// <returns></returns>
        public DateTime GetTokenExpires(string token)
        {

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                string expires = tokenHandler.ReadJwtToken(token).Claims.Where(c => c.Type == "exp").FirstOrDefault()?.Value;
                return TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local).AddSeconds(Convert.ToDouble(expires));
            }
            catch
            {
                return new DateTime(1970, 1, 1);
            }

        }

        /// <summary>
        /// 验证token是否有效,未超时且未被销毁
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsValid(string token)
        {
            return DateTime.Now < GetTokenExpires(token) && !TwinkleContext.Cache.Exists(token);
        }

        /// <summary>
        /// 把token推送到客户端headers["access-token"]
        /// </summary>
        /// <param name="token">要推送的token</param>
        public void PushToken(string token)
        {
            TwinkleContext.HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "access-token";
            TwinkleContext.HttpContext.Response.Headers["access-token"] = token;
        }

        /// <summary>
        /// 根据情况刷新token
        /// </summary>
        /// <param name="token">当前使用的有效token</param>
        /// <param name="context">上下文对象</param>
        public void AutoRefresh(string token, HttpContext context)
        {

        }

        /// <summary>
        /// 销毁token
        /// </summary>
        /// <param name="token"></param>
        public void DestroyToken(string token)
        {
            TwinkleContext.Cache.Set(token, "des-token", null, GetTokenExpires(token).AddSeconds(10));
        }
    }
}
