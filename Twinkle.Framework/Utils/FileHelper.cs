using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
