using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

public class InventoryQueueService : IInventoryQueueService
{
    private readonly QueueClient _queueClient;
    private readonly ILogger<InventoryQueueService> _logger;
    private const string QUEUE_NAME = "inventory-queue";

    public InventoryQueueService(ILogger<InventoryQueueService> logger)
    {
        _logger = logger;
        _queueClient = new QueueClient(Environment.GetEnvironmentVariable("BlobStorage"), QUEUE_NAME);
        _queueClient.CreateIfNotExists();
        _logger.LogInformation("Initialized InventoryQueueService with queue: {QueueName}", QUEUE_NAME);
    }

    public async Task QueueOrderItemMessage(ICollection<OrderItem> orderItems)
    {
        var messageJson = JsonSerializer.Serialize(orderItems);
        _logger.LogInformation("Queueing inventory message with content: {MessageContent}", messageJson);

        var base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(messageJson));
        _logger.LogInformation("Base64 encoded message: {Base64Message}", base64Message);

        await _queueClient.SendMessageAsync(base64Message);
        _logger.LogInformation("Successfully queued inventory message to {QueueName}", QUEUE_NAME);
    }
}