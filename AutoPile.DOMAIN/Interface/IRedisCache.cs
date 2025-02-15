using Microsoft.Extensions.Caching.Distributed;

namespace AutoPile.DATA.Cache
{
    public interface IRedisCache<T> where T : class
    {
        Task<T?> GetAsync(string key);

        Task RemoveAsync(string key);

        Task SetAsync(string key, T item, DistributedCacheEntryOptions cacheOptions);

        Task SetAsyncToList(string key, IEnumerable<T> item, DistributedCacheEntryOptions cacheOptions);

        Task<IEnumerable<T>?> GetAsyncToList(string key);

        Task RemoveAsyncToList(string key);
    }
}