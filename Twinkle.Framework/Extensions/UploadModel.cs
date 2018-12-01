using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Twinkle.Framework.Extensions
{
    public class UploadModel
    {
        /// <summary>
        /// 文件名称 不带拓展名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 拓展名
        /// </summary>
        public string Extension { get; internal set; }

        /// <summary>
        /// 文件全名 带拓展名
        /// </summary>
        public string FullFileName
        {
            get
            {
                return FileName + Extension;
            }
        }

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length { get; internal set; }

        /// <summary>
        /// 文件流
        /// </summary>
        public Stream FileStream { get; internal set; }

        /// <summary>
        /// 客制化信息
        /// </summary>
        public JObject CustomData { get; internal set; }
    }
}
