﻿using AutoMapper;
using AutoPile.DATA.Cache;
using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Interface;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace AutoPile.SERVICE.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IMapper _mapper;
        private readonly AutoPileMongoDbContext _autoPileMongoDbContext;
        private readonly AutoPileManagementDbContext _autoPileManagementDbContext;
        private readonly IBlobService _blobService;
        private readonly IReviewsCache _reviewsCache;

        public ReviewService(IMapper mapper, IBlobService blobService, AutoPileMongoDbContext autoPileMongoDbContext, AutoPileManagementDbContext autoPileManagementDbContext, IReviewsCache reviewsCache)
        {
            _mapper = mapper;
            _autoPileMongoDbContext = autoPileMongoDbContext;
            _autoPileManagementDbContext = autoPileManagementDbContext;
            _blobService = blobService;
            _reviewsCache = reviewsCache;
        }

        public async Task<ReviewResponseDTO> CreateReviewAsync(ReviewCreateDTO reviewCreateDTO, string applicationUserId)
        {
            if (string.IsNullOrEmpty(applicationUserId))
            {
                throw new BadRequestException("User Id is null");
            }

            var user = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {applicationUserId} not found");
            }

            if (!ObjectId.TryParse(reviewCreateDTO.ProductId, out ObjectId productObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }

            var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {reviewCreateDTO.ProductId} not found");
            }

            var review = _mapper.Map<Review>(reviewCreateDTO);
            review.UserId = user.Id;

            if (reviewCreateDTO.Image != null)
            {
                review.ImageUrl = await _blobService.UploadImageAsync(reviewCreateDTO.Image);
            }

            await _autoPileMongoDbContext.Reviews.AddAsync(review);
            await _autoPileMongoDbContext.SaveChangesAsync();

            var reviewResponseDto = _mapper.Map<ReviewResponseDTO>(review);

            return reviewResponseDto;
        }

        public async Task<ReviewResponseDTO> GetReviewByIdAsync(string ReviewId)
        {
            if (!ObjectId.TryParse(ReviewId, out ObjectId reviewObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }

            var review = await _autoPileMongoDbContext.Reviews.FindAsync(reviewObjectId) ?? throw new NotFoundException($"Review with ID {ReviewId} not found");
            return _mapper.Map<ReviewResponseDTO>(review);
        }

        public async Task<IEnumerable<ReviewResponseDTO>> GetReviewsByProductIdAsync(string ProductId)
        {
            if (!ObjectId.TryParse(ProductId, out ObjectId productObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }
            _ = await _autoPileMongoDbContext.Products.FindAsync(productObjectId) ?? throw new NotFoundException($"Product with ID {ProductId} not found");

            //var reviewCache = await _reviewsCache.GetReviewAsync(ProductId);
            //if (reviewCache != null)
            //{
            //    return reviewCache;
            //}

            var reviews = await _autoPileMongoDbContext.Reviews.Where(r => r.ProductId == productObjectId).ToListAsync();

            //if (reviews.Any())
            //{
            //    await _reviewsCache.SetReviewAsync(ProductId, _mapper.Map<IEnumerable<ReviewResponseDTO>>(reviews));
            //}

            return _mapper.Map<IEnumerable<ReviewResponseDTO>>(reviews);
        }

        public async Task<ReviewResponseDTO> UpdateReviewAsync(ReviewUpdateDTO reviewUpdateDTO, string reviewId, string applicationUserId)
        {
            _ = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId)
                ?? throw new NotFoundException($"User with ID {applicationUserId} not found");

            if (!ObjectId.TryParse(reviewId, out ObjectId reviewObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }

            var review = await _autoPileMongoDbContext.Reviews.FindAsync(reviewObjectId)
                ?? throw new NotFoundException($"Review with ID {reviewId} not found");

            if (review.UserId != applicationUserId)
            {
                throw new ForbiddenException("You are not authorized to update this review");
            }

            if (reviewUpdateDTO.Title != null)
                review.Title = reviewUpdateDTO.Title;
            if (reviewUpdateDTO.Subtitle != null)
                review.Subtitle = reviewUpdateDTO.Subtitle;
            if (reviewUpdateDTO.Content != null)
                review.Content = reviewUpdateDTO.Content;
            if (reviewUpdateDTO.Rating.HasValue)
                review.Rating = reviewUpdateDTO.Rating.Value;

            if (reviewUpdateDTO.Image != null)
            {
                var oldImageUrl = review.ImageUrl;
                if (string.IsNullOrEmpty(oldImageUrl))
                {
                    review.ImageUrl = await _blobService.UploadImageAsync(reviewUpdateDTO.Image);
                }
                else
                {
                    review.ImageUrl = await _blobService.UpdateImageAsync(oldImageUrl, reviewUpdateDTO.Image);
                }
            }
            review.UpdatedAt = DateTime.UtcNow;
            _autoPileMongoDbContext.Update(review);
            await _autoPileMongoDbContext.SaveChangesAsync();

            //await _reviewsCache.UpdateReviewAsync(_mapper.Map<ReviewResponseDTO>(review));
            return _mapper.Map<ReviewResponseDTO>(review);
        }

        public async Task DeleteReviewAsync(string reviewId, string applicationUserId)
        {
            _ = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId)
                ?? throw new NotFoundException($"User with ID {applicationUserId} not found");
            if (!ObjectId.TryParse(reviewId, out ObjectId reviewObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }
            var review = await _autoPileMongoDbContext.Reviews.FindAsync(reviewObjectId)
               ?? throw new NotFoundException($"Review with ID {reviewId} not found");
            if (review.UserId != applicationUserId)
            {
                throw new ForbiddenException("You are not authorized to delete this review");
            }
            if (review.ImageUrl != null) { await _blobService.DeleteImageAsync(review.ImageUrl); }
            _autoPileMongoDbContext.Remove(review);
            await _autoPileMongoDbContext.SaveChangesAsync();

            //await _reviewsCache.DeleteReviewAsync(review.ProductId.ToString(), reviewId);
        }
    }
}