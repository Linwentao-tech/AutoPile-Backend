using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;

namespace AutoPile.SERVICE.Services
{
    public interface IShoppingCartItemService
    {
        Task<ShoppingCartItemResponseDTO> CreateShoppingCartItemAsync(ShoppingCartItemRequestDto shoppingCartItemRequest, string applicationUserId);

        Task DeleteShoppingCartItemAsync(int shoppingCartItemId, string applicationUserId);

        Task<ShoppingCartItemResponseDTO> GetShoppingCartItemById(int shoppingCartItemId, string applicationUserId);

        Task UpdateShoppingCartItemAsync(UpdateShoppingCartItemDto updateShoppingCartItemDto, int shoppingCartItemId, string? applicationUserId);

        Task DeleteAllShoppingCartItemsAsync(string applicationUserId);

        Task<IEnumerable<ShoppingCartItemResponseDTO>> GetShoppingCartItemByUserId(string applicationUserId);
    }
}