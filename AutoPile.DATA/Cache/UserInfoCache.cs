using AutoPile.DOMAIN.DTOs.Responses;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Cache
{
    public class UserInfoCache : IUserInfoCache
    {
        private readonly IRedisCache<UserInfoResponseDTO> _redisCache;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public UserInfoCache(IRedisCache<UserInfoResponseDTO> redisCache)
        {
            _redisCache = redisCache;
            _redisCache = redisCache;
            _cacheOptions = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromHours(12)
            };
        }

        public async Task SetUserAsync(string applicationUserId, UserInfoResponseDTO userInfoResponseDTO)
        {
            await _redisCache.SetAsync(CacheKeys.User(applicationUserId), userInfoResponseDTO, _cacheOptions);
        }

        public async Task<UserInfoResponseDTO?> GetUserAsync(string applicationUserId)
        {
            return await _redisCache.GetAsync(CacheKeys.User(applicationUserId));
        }
    }
}