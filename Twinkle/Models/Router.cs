using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Twinkle.Models
{
    public class Router
    {
        /// <summary>
        /// 映射路由地址
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 模块文件地址
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 模块标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 模块图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 仅为菜单根目录下的独立菜单有效 为true则默认跳转出当前路由框架,为false的时候作为内嵌的框架页面显示
        /// </summary>
        public bool IsSingle { get; set; } = false;
        /// <summary>
        /// 子模块信息
        /// </summary>
        public List<Router> Children { get; set; }
    }
}
