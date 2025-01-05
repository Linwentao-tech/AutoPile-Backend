using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models;
using AutoPile.SERVICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace AutoPile.API.Controllers
{
    /// <summary>
    /// Controller for managing product reviews
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        /// <summary>
        /// Initializes a new instance of the ReviewController
        /// </summary>
        /// <param name="reviewService">The review service for handling business logic</param>
        /// <param name="logger">The logger for recording operations</param>
        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new product review
        /// </summary>
        /// <param name="reviewCreateDTO">The review details to create</param>
        /// <returns>The newly created review</returns>
        /// <response code="200">Returns the newly created review</response>
        /// <response code="400">If the review data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost("CreateReview", Name = "CreateReview")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateReview(ReviewCreateDTO reviewCreateDTO)
        {
            try
            {
                var userId = HttpContext.Items["UserId"]?.ToString();
                _logger.LogInformation($"Attempting to create review with userId: {userId}");

                var responseReviewDTO = await _reviewService.CreateReviewAsync(reviewCreateDTO, userId);
                _logger.LogInformation($"Review created successfully with ID {responseReviewDTO.Id}");

                return ApiResponse<ReviewResponseDTO>.OkResult(responseReviewDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific review by its ID
        /// </summary>
        /// <param name="Id">The ID of the review to retrieve</param>
        /// <returns>The requested review</returns>
        /// <response code="200">Returns the requested review</response>
        /// <response code="404">If the review is not found</response>
        [HttpGet("GetReviewById", Name = "GetReviewById")]
        public async Task<IActionResult> GetReviewById(string Id)
        {
            var responseReviewDTO = await _reviewService.GetReviewByIdAsync(Id);
            _logger.LogInformation("Successfully retrieved review {ReviewId}", Id);
            return ApiResponse<ReviewResponseDTO>.OkResult(responseReviewDTO);
        }

        /// <summary>
        /// Retrieves all reviews for a specific product
        /// </summary>
        /// <param name="productId">The ID of the product to get reviews for</param>
        /// <returns>A collection of reviews for the specified product</returns>
        /// <response code="200">Returns the list of reviews</response>
        /// <response code="404">If the product is not found</response>
        [HttpGet("GetReviewByProductId", Name = "GetReviewByProductId")]
        public async Task<IActionResult> GetReviewsByProductId(string productId)
        {
            var responseReviewDTO = await _reviewService.GetReviewsByProductIdAsync(productId);
            _logger.LogInformation("Successfully retrieved {Count} reviews for product {ProductId}",
                   responseReviewDTO?.Count() ?? 0, productId);
            return ApiResponse<IEnumerable<ReviewResponseDTO>>.OkResult(responseReviewDTO);
        }

        /// <summary>
        /// Updates an existing review
        /// </summary>
        /// <param name="reviewUpdateDTO">The updated review data</param>
        /// <param name="id">The ID of the review to update</param>
        /// <returns>The updated review</returns>
        /// <response code="200">Returns the updated review</response>
        /// <response code="400">If the update data is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to update the review</response>
        /// <response code="404">If the review is not found</response>
        [HttpPatch("{id}", Name = "UpdateReviewByReviewId")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateReview(ReviewUpdateDTO reviewUpdateDTO, string id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var response = await _reviewService.UpdateReviewAsync(reviewUpdateDTO, id, userId);
            _logger.LogInformation("Successfully updated review {ReviewId}", id);
            return ApiResponse<ReviewResponseDTO>.OkResult(response, "Resource updated successfully");
        }

        /// <summary>
        /// Deletes a specific review
        /// </summary>
        /// <param name="id">The ID of the review to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the review was successfully deleted</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="403">If the user is not authorized to delete the review</response>
        /// <response code="404">If the review is not found</response>
        [HttpDelete("{id}", Name = "DeleteReviewByReviewId")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteReview(string id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _reviewService.DeleteReviewAsync(id, userId);
            _logger.LogInformation("Successfully deleted review {ReviewId}", id);
            return NoContent();
        }
    }
}