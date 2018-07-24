using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Twinkle.Framework.Mvc
{
    public class UploadFileArgs
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
        public string FileFullName
        {
            get
            {
                return FileName + Extension;
            }
        }

        /// <summary>
        /// 源文件名 带拓展名,在修改过FileName后,该文件名不会变化
        /// </summary>
        public string OriginFullName { get; internal set; }

        /// <summary>
        /// 源文件名 不带拓展名,在修改过FileName后,该文件名不会变化
        /// </summary>
        public string OriginName { get; internal set; }

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

        /// <summary>
        /// 文件保存后 对应的物理路径地址
        /// </summary>
        public string Physical { get; private set; }

        /// <summary>
        /// 文件保存后 对应的虚拟路径地址,仅保存在WebRoot下的文件才会有虚拟路径
        /// </summary>
        public string Virtual { get; private set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="savePath">文件保存目录 不写默认配置文件的上传根目录</param>
        /// <param name="isWebRoot">是否放在网站根目录(wwwroot)</param>
        public async Task<bool> Save(string savePath = "", bool isWebRoot = false)
        {

            string RootFolder = TwinkleContext.UserConfig.GetValue<string>("Upload:RootFolder");

            savePath = Path.Combine(isWebRoot ? TwinkleContext.WebRoot : TwinkleContext.AppRoot, RootFolder, savePath);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }


            FileName = Guid.NewGuid().ToString("N");

            string filePath = Path.Combine(savePath, FileFullName);

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                FileStream.CopyTo(fs);
                await fs.FlushAsync();
                Physical = filePath;
                if (isWebRoot)
                {
                    Virtual = filePath.Replace(TwinkleContext.WebRoot, "").Replace("\\", "/");
                }
                return true;
            }
        }

        public void SendStatus(string message)
        {
          //  HubServer<UploadMessageHub>.Instance.SendAll("ServiceStatus", message);
        }
    }
}
