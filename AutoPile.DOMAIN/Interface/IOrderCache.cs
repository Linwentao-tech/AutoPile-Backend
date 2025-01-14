using AutoPile.DOMAIN.DTOs.Responses;

namespace AutoPile.DATA.Cache
{
    public interface IOrderCache
    {
        Task DeleteOrderAsync(string applicationUserId);
        Task<IEnumerable<OrderResponseDTO>?> GetOrderAsync(string applicationUserId);
        Task SetOrderAsync(string applicationUserId, IEnumerable<OrderResponseDTO> orderResponseDTOs);
        Task UpdateOrderAsync(OrderResponseDTO orderResponseDTO);
    }
}