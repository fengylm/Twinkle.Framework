using System;
using System.Threading.Tasks;

namespace Twinkle.Framework.Cache
{
    public interface ICacheService
    {

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="slidingExpiration">平滑过期时间（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="AbsoluteExpiration">绝对过期时间</param>
        void Set(string key, object value, TimeSpan? slidingExpiration = null, DateTimeOffset? AbsoluteExpiration = null);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="slidingExpiration">平滑过期时间（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="AbsoluteExpiration">绝对过期时间</param>
        Task SetAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTimeOffset? AbsoluteExpiration = null);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        string Get(string key);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        Task<string> GetAsync(string key);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        void Remove(string key);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 刷新缓存,延长过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        void Refresh(string key);

        /// <summary>
        /// 刷新缓存,延长过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        Task RefreshAsync(string key);
        /// <summary>
        /// 检测缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        bool Exists(string key);
        /// <summary>
        /// 检测缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string key);
    }
}
