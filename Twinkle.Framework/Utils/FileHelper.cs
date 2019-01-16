using System;

namespace Twinkle.Framework.Utils
{
    public static class FileHelper
    {
        /// <summary>
        /// 带文件名称的全路径地址
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string FileName(string filePath)
        {
            filePath = filePath.Replace("\\", "/");
            return filePath.RightOfLast("/");
        }

        /// <summary>
        /// 获取文件后缀名
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="withDot">是否包含后缀名前的点符号</param>
        /// <returns></returns>
        public static string FileExtension(string fileName, bool withDot = false)
        {
            string[] step = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (step.Length == 0)
            {
                return "";
            }
            if (withDot)
            {
                return "." + step[step.Length - 1];
            }
            else
            {
                return step[step.Length - 1];
            }
        }
    }
}
