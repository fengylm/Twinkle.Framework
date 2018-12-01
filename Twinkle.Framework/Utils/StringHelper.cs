namespace Twinkle.Framework.Utils
{
    public static class StringHelper
    {
        /// <summary>
        /// 获取最后一次出现的指定字符右边的数据(不包含指定字符)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lastStr">指定字符</param>
        /// <returns></returns>
        public static string RightOfLast(this string value, string lastStr)
        {
            int index = value.LastIndexOf(lastStr);
            if (index != -1)
            {
                return value.Substring(index + 1);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 获取最后一次出现的指定字符左边的数据(不包含指定字符)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lastStr">指定字符</param>
        /// <returns></returns>
        public static string LeftOfLast(this string value, string lastStr)
        {
            int index = value.LastIndexOf(lastStr);
            if (index != -1)
            {
                return value.Substring(0, index);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 获取一次出现的指定字符右边的数据(不包含指定字符)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="str">指定字符</param>
        /// <returns></returns>
        public static string RightOf(this string value, string str)
        {
            int index = value.IndexOf(str);
            if (index != -1)
            {
                return value.Substring(index + 1);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 获取一次出现的指定字符左边的数据(不包含指定字符)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="str">指定字符</param>
        /// <returns></returns>
        public static string LeftOf(this string value, string str)
        {
            int index = value.IndexOf(str);
            if (index != -1)
            {
                return value.Substring(0, index);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 判断字符串是否空
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}
