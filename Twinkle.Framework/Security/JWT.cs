using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Twinkle.Framework.Cache;
using Twinkle.Framework.Mvc;

namespace Twinkle.Framework.Security
{
    public class JWT
    {
        private string securityKey;
        private int expires;
        internal JWT(string SecurityKey, int Expires)
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
        public string CreateToken(object UserData, int? Expires = null)
        {
            #region 验证UserData的合规性
            if (UserData == null)
            {
                throw new InvalidCastException("UserData必须是带有uid节点的有效json格式");
            }
            JToken userData = null;
            try
            {
                userData = JToken.FromObject(UserData);
                if (string.IsNullOrEmpty(userData.Value<string>("uid")))
                {
                    throw new InvalidCastException("UserData必须是带有uid节点的有效json格式");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            #endregion
            int maxExpires = (Expires ?? expires) > 120 ? 120 : Expires ?? expires;
            Claim[] claim = new Claim[]{
                    new Claim(ClaimTypes.UserData, userData.ToString()),
                    new Claim(ClaimTypes.Expired,DateTime.Now.AddMinutes(Expires??expires).ToString("yyyy-MM-dd HH:mm:ss"))
                };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    "Twinkle",
                    "TwinkleClient",
                    claim,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(maxExpires),
                    creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 获取UserData的json对象
        /// </summary>
        /// <param name="token">token字符串</param>
        public string GetUserData(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                return tokenHandler.ReadJwtToken(token).Claims.Where(c => c.Type == ClaimTypes.UserData).FirstOrDefault()?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取UserData的实例对象
        /// </summary>
        /// <typeparam name="T">要转换成的对象</typeparam>
        /// <param name="token"></param>
        /// <returns></returns>
        public T GetUserData<T>(string token)
        {
            return JToken.Parse(GetUserData(token)).ToObject<T>();
        }

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
        public bool Valid(string token)
        {
            RedisService rs = TwinkleContext.GetService<RedisService>();
            return DateTime.Now < GetTokenExpires(token) && !rs.Exists(token);
        }

        /// <summary>
        /// 把token推送到客户端headers["access-token"]
        /// </summary>
        /// <param name="token">要推送的token</param>
        public void PushToken(string token)
        {
            TwinkleContext.MvcHttpContext.Response.Headers["Access-Control-Expose-Headers"] = "access-token";
            TwinkleContext.MvcHttpContext.Response.Headers["access-token"] = token;
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
            RedisService rs = TwinkleContext.GetService<RedisService>();
            JWT jwt = TwinkleContext.GetService<JWT>();
            rs.Set(token, "", jwt.GetTokenExpires(token).AddSeconds(10));
        }
    }
}
