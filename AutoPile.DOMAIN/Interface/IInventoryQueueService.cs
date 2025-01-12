using AutoPile.DOMAIN.Models.Entities;

namespace AutoPile.SERVICE.Services
{
    public interface IInventoryQueueService
    {
        Task QueueOrderItemMessage(ICollection<OrderItem> orderItems);
    }
}