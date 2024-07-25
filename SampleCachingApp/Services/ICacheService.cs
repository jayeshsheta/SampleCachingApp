using Microsoft.Extensions.Caching.Memory;

namespace SampleCachingApp.Services
{
    public interface ICacheService
    {
        T Get<T>(string key);
        void Set<T>(string key, T item);
        void Invalidate(string key);
    }

    public class MemoryCacheService : ICacheService
    {
        private readonly MemoryCache _cache;

        public MemoryCacheService()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public T Get<T>(string key)
        {
            return _cache.TryGetValue(key, out T value) ? value : default;
        }

        public void Set<T>(string key, T item)
        {
            _cache.Set(key, item);
        }

        public void Invalidate(string key)
        {
            _cache.Remove(key);
        }
    }

}
