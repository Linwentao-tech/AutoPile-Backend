using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;

namespace AutoPile.SERVICE.Services
{
    public interface IOrderItemService
    {
        Task<OrderItemResponseDTO> CreateOrderItemAsync(OrderItemCreateDTO orderItemCreateDTO);
        Task DeleteOrderItem(string id);
        Task<OrderItemResponseDTO> GetOrderItemById(string id);
        Task<OrderItemResponseDTO> UpdateOrderItem(OrderItemUpdateDTO orderItemUpdateDTO, string id);
    }
}