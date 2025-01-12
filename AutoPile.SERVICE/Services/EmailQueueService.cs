using AutoPile.DOMAIN.Models.MessageQueue;
using Azure.Storage.Queues;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly QueueClient _queueClient;

        public EmailQueueService()
        {
            _queueClient = new QueueClient(Environment.GetEnvironmentVariable("BlobStorage"), "email-queue");
            _queueClient.CreateIfNotExists();
        }

        public async Task QueueEmailMessage(EmailMessage message)
        {
            var messageJson = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(
                Encoding.UTF8.GetBytes(messageJson)));
        }
    }
}