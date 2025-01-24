using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Responses;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Cache
{
    public class ReviewsCache : IReviewsCache
    {
        private readonly IRedisCache<ReviewResponseDTO> _redisCache;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private readonly AutoPileMongoDbContext _mongoContext;

        public ReviewsCache(IRedisCache<ReviewResponseDTO> redisCache, AutoPileMongoDbContext mongoContext)
        {
            _redisCache = redisCache;
            _cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            };
            _mongoContext = mongoContext;
        }

        public async Task<IEnumerable<ReviewResponseDTO>?> GetReviewAsync(string productId)
        {
            return await _redisCache.GetAsyncToList(CacheKeys.Review(productId));
        }

        public async Task SetReviewAsync(string productId, IEnumerable<ReviewResponseDTO> reviewResponseDTO)
        {
            await _redisCache.SetAsyncToList(CacheKeys.Review(productId), reviewResponseDTO, _cacheOptions);
        }

        public async Task DeleteReviewAsync(string productId, string reviewId)
        {
            var reviewList = await _redisCache.GetAsyncToList(CacheKeys.Review(productId));
            if (reviewList != null)
            {
                var review = reviewList.FirstOrDefault(rv => rv.Id == reviewId);
                if (review != null)
                {
                    reviewList.ToList().Remove(review);
                }
            }
            if (reviewList == null)
            {
                await _redisCache.RemoveAsync(CacheKeys.Review(productId));
            }
        }

        public async Task UpdateReviewAsync(ReviewResponseDTO reviewResponseDTO)
        {
            var reviewList = await _redisCache.GetAsyncToList(CacheKeys.Review(reviewResponseDTO.ProductId));

            if (reviewList != null)
            {
                var review = reviewList.FirstOrDefault(rv => rv.Id == reviewResponseDTO.Id);
                if (review != null)
                {
                    reviewList.ToList().Remove(review);
                    reviewList.ToList().Add(reviewResponseDTO);
                    await SetReviewAsync(reviewResponseDTO.ProductId, reviewList);
                }
            }
        }
    }
}