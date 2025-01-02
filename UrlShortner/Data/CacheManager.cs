using Microsoft.Extensions.Caching.Memory;

namespace UrlShortner.Data
{
    public class CacheManager
    {
        private readonly IMemoryCache _memoryCache;
        public CacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T Get<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out T value))
            {
                return value;
            }

            return default;
        }

        public void Set<T>(string key, T value, MemoryCacheEntryOptions options = null)
        {
            _memoryCache.Set(key, value, options ?? CacheOptions.DefaultCacheOptions);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }
    }

    public static class CacheOptions
    {
        public static MemoryCacheEntryOptions DefaultCacheOptions => new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),  // Universal absolute expiration
            SlidingExpiration = TimeSpan.FromMinutes(15),           // Universal sliding expiration
            Priority = CacheItemPriority.Normal                     // Universal cache priority
        };
    }
}
