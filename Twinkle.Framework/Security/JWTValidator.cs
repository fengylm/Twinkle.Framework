using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Twinkle.Framework.Cache;
using Twinkle.Framework.Mvc;

namespace Twinkle.Framework.Security
{
    public class JWTValidator : JwtSecurityTokenHandler
    {
        public override ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            RedisService rs = TwinkleContext.GetService<RedisService>();
            //如果是已经拉入黑名单的token则直接提示验证失败
            if (!rs.Exists(securityToken))
            {
                //后期可以在这里处理token刷新
                try
                {
                    return base.ValidateToken(securityToken, validationParameters, out validatedToken);
                }
                catch
                {
                    //过期或者签名错误的验证会引发异常,这里返回一个啥都不包含的ClaimsPrincipal即可使网页响应为404
                    validatedToken = null;
                    return new ClaimsPrincipal();
                }
            }
            validatedToken = null;
            return new ClaimsPrincipal();
        }
    }
}
