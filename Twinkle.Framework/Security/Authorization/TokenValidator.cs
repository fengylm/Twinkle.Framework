using Twinkle.Framework.Cache;
using Twinkle.Framework.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Twinkle.Framework.Security.Authorization
{
    public class TokenValidator : JwtSecurityTokenHandler
    {
        public override ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            //如果是已经拉入黑名单的token则直接提示验证失败
            if (!TwinkleContext.Cache.Exists(securityToken))
            {
                //后期可以在这里处理token刷新
                try
                {
                    return base.ValidateToken(securityToken, validationParameters, out validatedToken);
                }
                catch
                {
                    //过期或者签名错误的验证会引发异常,这里返回一个啥都不包含的ClaimsPrincipal即可使网页响应为401
                    validatedToken = null;
                    return new ClaimsPrincipal();
                }
            }
            validatedToken = null;
            return new ClaimsPrincipal();
        }
    }
}
