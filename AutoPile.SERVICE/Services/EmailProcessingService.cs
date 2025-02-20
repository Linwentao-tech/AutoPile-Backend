// EmailProcessingService.cs
using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resend;
using System.Text.Json;
using System.Text;

public class EmailProcessingService : BackgroundService
{
    private readonly QueueClient _queueClient;
    private readonly IResend _resend;
    private readonly ILogger<EmailProcessingService> _logger;
    private const string QUEUE_NAME = "email-queue";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public EmailProcessingService(QueueClient queueClient, IResend resend, ILogger<EmailProcessingService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Initializing EmailProcessingService - {Time}", DateTime.UtcNow);

        try
        {
            _queueClient = queueClient;
            _logger.LogInformation("Queue client injected successfully");

            _resend = resend;
            _logger.LogInformation("EmailProcessingService initialization complete");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "EmailProcessingService initialization failed");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailProcessingService background task starting - {Time}", DateTime.UtcNow);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Polling for messages - {Time}", DateTime.UtcNow);
                var messages = await _queueClient.ReceiveMessagesAsync(maxMessages: 32, cancellationToken: stoppingToken);

                _logger.LogInformation("Received {Count} messages", messages.Value.Count());

                foreach (var message in messages.Value)
                {
                    try
                    {
                        var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                        _logger.LogDebug("Processing message: {MessageId}", message.MessageId);

                        var emailMessage = JsonSerializer.Deserialize<AutoPile.DOMAIN.Models.MessageQueue.EmailMessage>(decodedString, _jsonOptions);
                        if (emailMessage == null)
                        {
                            _logger.LogWarning("Failed to deserialize message {MessageId}", message.MessageId);
                            continue;
                        }

                        var resendMessage = new Resend.EmailMessage
                        {
                            From = $"{emailMessage.MessageType}@autopile.store",
                            To = { emailMessage.To },
                            Subject = emailMessage.Subject,
                            HtmlBody = emailMessage.Body
                        };

                        await _resend.EmailSendAsync(resendMessage);
                        _logger.LogInformation("Successfully sent email to {Recipient}", emailMessage.To);

                        await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                        _logger.LogInformation("Deleted processed message {MessageId}", message.MessageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in EmailProcessingService");
            throw;
        }
    }
}