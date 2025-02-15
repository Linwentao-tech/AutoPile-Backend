using AutoPile.DATA.Data;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.Models.Entities;
using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Text;
using System.Text.Json;

public class InventoryProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly QueueClient _queueClient;
    private readonly ILogger<InventoryProcessingService> _logger;
    private const string QUEUE_NAME = "inventory-queue";

    public InventoryProcessingService(IServiceScopeFactory serviceScopeFactory, ILogger<InventoryProcessingService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _logger.LogInformation("Initializing InventoryProcessingService with queue: {QueueName}", QUEUE_NAME);
        _queueClient = new QueueClient(Environment.GetEnvironmentVariable("BlobStorage"), QUEUE_NAME);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting InventoryProcessingService background task");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug("InventoryProcessingService: Checking for messages in {QueueName}", QUEUE_NAME);
            var messages = await _queueClient.ReceiveMessagesAsync(maxMessages: 32, cancellationToken: stoppingToken);

            if (messages.Value.Any())
            {
                _logger.LogInformation("InventoryProcessingService: Found {MessageCount} messages in {QueueName}",
                    messages.Value.Count(), QUEUE_NAME);

                foreach (var message in messages.Value)
                {
                    _logger.LogInformation("InventoryProcessingService: Received message {MessageId} from {QueueName}",
                        message.MessageId, QUEUE_NAME);

                    var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                    _logger.LogInformation("InventoryProcessingService: Decoded message content: {DecodedMessage}", decodedString);
                    var orderItems = JsonSerializer.Deserialize<List<OrderItem>>(decodedString);
                    _logger.LogInformation("InventoryProcessingService: Successfully deserialized message {MessageId}", message.MessageId);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var mongoContext = scope.ServiceProvider.GetRequiredService<AutoPileMongoDbContext>();
                        foreach (var item in orderItems)
                        {
                            _logger.LogInformation("Processing Item: {Id}, {ProductName}, {Quantity}", item.Id, item.ProductName, item.Quantity);
                            if (!ObjectId.TryParse(item.ProductId, out ObjectId productObjectId))
                            {
                                _logger.LogError("Invalid product ID format: {ProductId}", item.ProductId);
                                continue;
                            }

                            var product = await mongoContext.Products.FindAsync(productObjectId);
                            if (product == null)
                            {
                                _logger.LogError("Product not found: {ProductId}", item.ProductId);
                                continue;
                            }
                            if (product.StockQuantity > item.Quantity)
                            {
                                product.StockQuantity -= item.Quantity;
                                product.IsInStock = product.StockQuantity > 0;
                                product.UpdatedAt = DateTime.UtcNow;
                                mongoContext.Update(product);
                                await mongoContext.SaveChangesAsync(stoppingToken);
                                _logger.LogInformation(
                   "Successfully updated inventory for product {ProductId}. New stock: {StockQuantity}",
                   item.ProductId, product.StockQuantity);
                            }
                        }
                    }

                    await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                    _logger.LogInformation("InventoryProcessingService: Deleted message {MessageId} from {QueueName}",
                        message.MessageId, QUEUE_NAME);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}