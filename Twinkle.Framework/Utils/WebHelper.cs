using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Twinkle.Framework.Mvc;

namespace Twinkle.Framework.Utils
{
    public static class WebHelper
    {
        /// <summary>
        /// 基于前台tw-export组件实现的文件下载
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileName">前台显示的文件名</param>
        /// <returns></returns>
        public static IActionResult DownLoad(Stream stream,string fileName)
        {
            TwinkleContext.MvcHttpContext.Response.Headers["Access-Control-Expose-Headers"] = "filename";
            TwinkleContext.MvcHttpContext.Response.Headers["filename"] = UrlEncoder.Default.Encode(fileName);
            return new FileStreamResult(stream, "application/octet-stream");
        }

        /// <summary>
        /// 基于前台tw-export组件实现的文件下载
        /// </summary>
        /// <param name="filePath">带文件名称的全路径地址</param>
        /// <returns></returns>
        public static IActionResult DownLoad(string filePath)
        {
            string fileName = FileHelper.FileName(filePath);
            return DownLoad(new FileStream(filePath, FileMode.Open), fileName);
        }
    }
}
