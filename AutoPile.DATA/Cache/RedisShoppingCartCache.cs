using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoPile.DATA.Cache
{
    public class RedisShoppingCartCache : IRedisShoppingCartCache
    {
        private readonly IDistributedCache _redis;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public RedisShoppingCartCache(IDistributedCache redis)
        {
            _redis = redis;
            _cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7),
                SlidingExpiration = TimeSpan.FromHours(24)
            };
        }

        public async Task<ShoppingCartItem?> GetItemAsync(string userId, int itemId)
        {
            var cart = await GetUserCartAsync(userId);
            return cart?.FirstOrDefault(i => i.Id == itemId);
        }

        public async Task<List<ShoppingCartItem>> GetUserCartAsync(string userId)
        {
            var key = CacheKeys.ShoppingCart(userId);
            var cart = await _redis.GetStringAsync(key);

            if (string.IsNullOrEmpty(cart))
                return new List<ShoppingCartItem>();
            await _redis.RefreshAsync(key);
            return JsonSerializer.Deserialize<List<ShoppingCartItem>>(cart);
        }

        public async Task SetItemAsync(ShoppingCartItem shoppingCartItem)
        {
            var cart = await GetUserCartAsync(shoppingCartItem.UserId);
            var existingItem = cart.FirstOrDefault(i => i.Id == shoppingCartItem.Id);
            if (existingItem != null)
            {
                cart.Remove(existingItem);
                cart.Add(shoppingCartItem);
            }
            else
            {
                cart.Add(shoppingCartItem);
            }
            var key = CacheKeys.ShoppingCart(shoppingCartItem.UserId);
            await _redis.SetStringAsync(key, JsonSerializer.Serialize(cart), _cacheOptions);
        }

        public async Task RemoveItemAsync(string userId, int itemId)
        {
            var cart = await GetUserCartAsync(userId);
            var existingItem = cart.FirstOrDefault(i => i.Id == itemId);
            if (existingItem != null)
            {
                cart.Remove(existingItem);
                var key = CacheKeys.ShoppingCart(userId);
                await _redis.SetStringAsync(key, JsonSerializer.Serialize(cart), _cacheOptions);
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var key = CacheKeys.ShoppingCart(userId);
            await _redis.RemoveAsync(key);
        }
    }
}