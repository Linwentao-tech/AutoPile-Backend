using AutoPile.DOMAIN.DTOs.Responses;

namespace AutoPile.DATA.Cache
{
    public interface IProductCache
    {
        Task DeleteProductAsync(string productId);
        Task<ProductResponseDTO?> GetProductAsync(string productId);
        Task SetProductAsync(string productId, ProductResponseDTO productResponseDTO);
    }
}