using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;

namespace AutoPile.SERVICE.Services
{
    public interface IShoppingCartItemService
    {
        Task<ShoppingCartItemResponseDTO> CreateShoppingCartItemAsync(ShoppingCartItemRequestDto shoppingCartItemRequest, string? applicationUserId);

        Task DeleteShoppingCartItemAsync(int shoppingCartItemId);

        Task<ShoppingCartItemResponseDTO> GetShoppingCartItemById(int shoppingCartItemId);

        Task UpdateShoppingCartItemAsync(UpdateShoppingCartItemDto updateShoppingCartItemDto, int shoppingCartItemId, string? applicationUserId);
    }
}