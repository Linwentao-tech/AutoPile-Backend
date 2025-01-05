using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;

namespace AutoPile.SERVICE.Services
{
    public interface IProductService
    {
        Task<ProductResponseDTO> CreateProductAsync(ProductCreateDTO productCreateDTO);

        Task<ProductResponseDTO> GetProductByIdAsync(string id);

        Task DeleteProductByIdAsync(string id);

        Task<ProductResponseDTO> UpdateProductByIdAsync(ProductUpdateDTO productUpdateDTO, string id);
    }
}