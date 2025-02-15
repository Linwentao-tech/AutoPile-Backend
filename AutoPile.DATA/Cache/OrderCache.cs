using AutoPile.DOMAIN.DTOs.Responses;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Cache
{
    public class OrderCache : IOrderCache
    {
        private readonly IRedisCache<OrderResponseDTO> _redisCache;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public OrderCache(IRedisCache<OrderResponseDTO> redisCache)
        {
            _redisCache = redisCache;
            _cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(3),
            };
        }

        public async Task<IEnumerable<OrderResponseDTO>?> GetOrderAsync(string applicationUserId)
        {
            return await _redisCache.GetAsyncToList(CacheKeys.Order(applicationUserId));
        }

        public async Task SetOrderAsync(string applicationUserId, IEnumerable<OrderResponseDTO> orderResponseDTOs)
        {
            await _redisCache.SetAsyncToList(CacheKeys.Order(applicationUserId), orderResponseDTOs, _cacheOptions);
        }

        public async Task DeleteOrderAsync(string applicationUserId)
        {
            await _redisCache.RemoveAsyncToList(CacheKeys.Order(applicationUserId));
        }

        public async Task UpdateOrderAsync(OrderResponseDTO orderResponseDTO)
        {
            var orderList = await _redisCache.GetAsyncToList(CacheKeys.Review(orderResponseDTO.UserId));

            if (orderList != null)
            {
                var order = orderList.FirstOrDefault(o => o.Id == orderResponseDTO.Id);
                if (order != null)
                {
                    orderList.ToList().Remove(order);
                    orderList.ToList().Add(orderResponseDTO);
                    await SetOrderAsync(orderResponseDTO.UserId, orderList);
                }
            }
        }
    }
}