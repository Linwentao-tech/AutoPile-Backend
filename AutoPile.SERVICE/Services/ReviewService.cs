using AutoMapper;
using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IMapper _mapper;
        private readonly AutoPileMongoDbContext _autoPileMongoDbContext;
        private readonly AutoPileManagementDbContext _autoPileManagementDbContext;
        private readonly IBlobService _blobService;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IMapper mapper, IBlobService blobService, ILogger<ReviewService> logger, AutoPileMongoDbContext autoPileMongoDbContext, AutoPileManagementDbContext autoPileManagementDbContext)
        {
            _mapper = mapper;
            _autoPileMongoDbContext = autoPileMongoDbContext;
            _autoPileManagementDbContext = autoPileManagementDbContext;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<ReviewResponseDTO> CreateReviewAsync(ReviewCreateDTO reviewCreateDTO, string applicationUserId)
        {
            try
            {
                _logger.LogInformation("Starting review creation for user {UserId}", applicationUserId);

                if (string.IsNullOrEmpty(applicationUserId))
                {
                    _logger.LogWarning("Attempt to create review with null user ID");
                    throw new BadRequestException("User Id is null");
                }

                var user = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found while creating review", applicationUserId);
                    throw new NotFoundException($"User with ID {applicationUserId} not found");
                }

                if (!ObjectId.TryParse(reviewCreateDTO.ProductId, out ObjectId productObjectId))
                {
                    _logger.LogWarning("Invalid product ID format: {ProductId}", reviewCreateDTO.ProductId);
                    throw new BadRequestException("Invalid product ID format");
                }

                var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found while creating review", reviewCreateDTO.ProductId);
                    throw new NotFoundException($"Product with ID {reviewCreateDTO.ProductId} not found");
                }

                _logger.LogDebug("Mapping review DTO to entity");
                var review = _mapper.Map<Review>(reviewCreateDTO);
                review.UserId = user.Id;

                if (reviewCreateDTO.Image != null)
                {
                    _logger.LogInformation("Uploading image for review");
                    review.ImageUrl = await _blobService.UploadImageAsync(reviewCreateDTO.Image);
                    _logger.LogDebug("Image uploaded successfully: {ImageUrl}", review.ImageUrl);
                }

                _logger.LogDebug("Adding review to database");
                await _autoPileMongoDbContext.Reviews.AddAsync(review);
                await _autoPileMongoDbContext.SaveChangesAsync();

                _logger.LogDebug("Mapping review entity to response DTO");
                var reviewResponseDto = _mapper.Map<ReviewResponseDTO>(review);

                _logger.LogInformation("Successfully created review {ReviewId} for product {ProductId}", review.Id, reviewCreateDTO.ProductId);
                return reviewResponseDto;
            }
            catch (BadRequestException ex)
            {
                _logger.LogWarning(ex, "Bad request while creating review: {Message}", ex.Message);
                throw;
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Not found error while creating review: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating review for user {UserId}", applicationUserId);
                throw;
            }
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
            var reviews = await _autoPileMongoDbContext.Reviews.Where(r => r.ProductId == productObjectId).ToListAsync();
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
        }
    }
}