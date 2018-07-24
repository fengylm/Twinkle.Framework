using System;
using System.Text;

namespace Twinkle.Framework.Security
{
    public class MD5
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="orginStr">要加密的字符串</param>
        /// <returns></returns>
        public static string Encrypt(string orginStr)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(orginStr));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }
    }
}
