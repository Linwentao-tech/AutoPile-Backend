using AutoPile.DOMAIN.Models.MessageQueue;

namespace AutoPile.SERVICE.Services
{
    public interface IEmailQueueService
    {
        Task QueueEmailMessage(EmailMessage message);
    }
}