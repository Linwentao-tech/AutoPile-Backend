using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoPile.DATA.Cache.CacheRepository
{
    public class RedisCache<T> : IRedisCache<T> where T : class
    {
        private readonly IDistributedCache _redis;

        public RedisCache(IDistributedCache redis)
        {
            _redis = redis;
        }

        public async Task<T?> GetAsync(string key)
        {
            var cachedValue = await _redis.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                await _redis.RefreshAsync(key);
                return JsonSerializer.Deserialize<T>(cachedValue);
            }
            return default;
        }

        public async Task<IEnumerable<T>?> GetAsyncToList(string key)
        {
            var cachedValue = await _redis.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                await _redis.RefreshAsync(key);
                return JsonSerializer.Deserialize<IEnumerable<T>>(cachedValue);
            }
            return default;
        }

        public async Task SetAsync(string key, T item, DistributedCacheEntryOptions cacheOptions)
        {
            var cachedProduct = await GetAsync(key);
            if (cachedProduct != null)
            {
                await _redis.SetStringAsync(key, JsonSerializer.Serialize(item), cacheOptions);
            }
            else
            {
                await _redis.RemoveAsync(key);
                await _redis.SetStringAsync(key, JsonSerializer.Serialize(item), cacheOptions);
            }
        }

        public async Task SetAsyncToList(string key, IEnumerable<T> item, DistributedCacheEntryOptions cacheOptions)
        {
            var cachedProduct = await GetAsync(key);
            if (cachedProduct != null)
            {
                await _redis.SetStringAsync(key, JsonSerializer.Serialize(item), cacheOptions);
            }
            else
            {
                await _redis.RemoveAsync(key);
                await _redis.SetStringAsync(key, JsonSerializer.Serialize(item), cacheOptions);
            }
        }

        public async Task RemoveAsync(string key)
        {
            var cachedProduct = await GetAsync(key);
            if (cachedProduct != null)
            {
                await _redis.RemoveAsync(key);
            }
        }

        public async Task RemoveAsyncToList(string key)
        {
            var cachedProduct = await GetAsyncToList(key);
            if (cachedProduct != null)
            {
                await _redis.RemoveAsync(key);
            }
        }
    }
}