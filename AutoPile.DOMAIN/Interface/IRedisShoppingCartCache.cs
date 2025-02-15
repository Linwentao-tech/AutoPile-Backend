using AutoPile.DOMAIN.Models.Entities;

namespace AutoPile.DATA.Cache
{
    public interface IRedisShoppingCartCache
    {
        Task ClearCartAsync(string userId);
        Task<ShoppingCartItem?> GetItemAsync(string userId, int itemId);
        Task<List<ShoppingCartItem>> GetUserCartAsync(string userId);
        Task RemoveItemAsync(string userId, int itemId);
        Task SetItemAsync(ShoppingCartItem shoppingCartItem);
    }
}