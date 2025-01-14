using AutoPile.DOMAIN.DTOs.Responses;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Cache
{
    public class ProductCache : IProductCache
    {
        private readonly IRedisCache<ProductResponseDTO> _redisCache;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public ProductCache(IRedisCache<ProductResponseDTO> redisCache)
        {
            _redisCache = redisCache;
            _cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            };
        }

        public async Task<ProductResponseDTO?> GetProductAsync(string productId)
        {
            return await _redisCache.GetAsync(CacheKeys.Product(productId));
        }

        public async Task SetProductAsync(string productId, ProductResponseDTO productResponseDTO)
        {
            await _redisCache.SetAsync(CacheKeys.Product(productId), productResponseDTO, _cacheOptions);
        }

        public async Task DeleteProductAsync(string productId)
        {
            await _redisCache.RemoveAsync(CacheKeys.Product(productId));
        }
    }
}