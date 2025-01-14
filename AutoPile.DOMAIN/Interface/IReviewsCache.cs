using AutoPile.DOMAIN.DTOs.Responses;

namespace AutoPile.DATA.Cache
{
    public interface IReviewsCache
    {
        Task DeleteReviewAsync(string productId);

        Task<IEnumerable<ReviewResponseDTO>?> GetReviewAsync(string productId);

        Task SetReviewAsync(string productId, IEnumerable<ReviewResponseDTO> reviewResponseDTO);

        Task UpdateReviewAsync(ReviewResponseDTO reviewResponseDTO);
    }
}