using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;

public interface IOrderService
{
    Task<OrderResponseDTO> CreateOrderAsync(OrderCreateDTO orderCreateDTO, string applicationUserId);

    Task<OrderResponseDTO> GetOrderByIdAsync(int orderId, string applicationUserId);

    Task<IEnumerable<OrderResponseDTO>> GetUserOrdersAsync(string applicationUserId);

    Task<OrderResponseDTO> UpdateOrderAsync(OrderUpdateDTO orderUpdateDTO, int orderId, string userId);

    Task DeleteOrderAsync(int orderId, string userId);

    Task<OrderResponseDTO> GetOrderByOrderIdAsync(string orderId, string applicationUserId);
}