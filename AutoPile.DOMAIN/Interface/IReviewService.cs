using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using MongoDB.Bson;

namespace AutoPile.SERVICE.Services
{
    public interface IReviewService
    {
        Task<ReviewResponseDTO> CreateReviewAsync(ReviewCreateDTO reviewCreateDTO, string applicationUserId);

        Task<ReviewResponseDTO> GetReviewByIdAsync(string ReviewId);

        Task<IEnumerable<ReviewResponseDTO>> GetReviewsByProductIdAsync(string ProductId);

        Task<ReviewResponseDTO> UpdateReviewAsync(ReviewUpdateDTO reviewUpdateDTO, string reviewId, string applicationUserId);

        Task DeleteReviewAsync(string reviewId, string applicationUserId);
    }
}