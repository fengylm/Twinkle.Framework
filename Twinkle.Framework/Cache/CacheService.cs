using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace Twinkle.Framework.Cache
{
    public class CacheService : ICacheService
    {
        private IDistributedCache mCache;

        /// <summary>
        /// 实例化缓存对象,推荐通过服务注入实现
        /// </summary>
        /// <param name="Cache"></param>
        public CacheService(IDistributedCache cache)
        {
            mCache = cache;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="slidingExpiration">平滑过期时间（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="AbsoluteExpiration">绝对过期时间</param>
        public void Set(string key, object value, TimeSpan? slidingExpiration = null, DateTimeOffset? AbsoluteExpiration = null)
        {
            var dco = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = AbsoluteExpiration
            };
            if (value == null || value.ToString() == "")
            {
                throw new ArgumentNullException("存储值不能为null或者空字符串");
            }
            mCache.SetString(key, value.ToString(), dco);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="slidingExpiration">平滑过期时间（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="AbsoluteExpiration">绝对过期时间</param>
        public Task SetAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTimeOffset? AbsoluteExpiration = null)
        {
            var dco = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = slidingExpiration,
                AbsoluteExpiration = AbsoluteExpiration
            };
            return mCache.SetStringAsync(key, value.ToString(), dco);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public string Get(string key)
        {
            return mCache.GetString(key);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public Task<string> GetAsync(string key)
        {
            return mCache.GetStringAsync(key);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        public void Remove(string key)
        {
            mCache.Remove(key);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        public Task RemoveAsync(string key)
        {
            return mCache.RemoveAsync(key);
        }

        /// <summary>
        /// 刷新缓存,延长过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        public void Refresh(string key)
        {
            mCache.Refresh(key);
        }

        /// <summary>
        /// 刷新缓存,延长过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        public Task RefreshAsync(string key)
        {
            return mCache.RefreshAsync(key);
        }
        /// <summary>
        /// 检测缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            return mCache.Get(key) != null;
        }
        /// <summary>
        /// 检测缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(string key)
        {
            return Task.Run(async () =>
            {
                return await mCache.GetAsync(key) != default(byte[]);
            });
        }
    }
}
