using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Resend;
using System.Text.Json;
using System.Text;
using AutoPile.DOMAIN.Models.MessageQueue;

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

    public EmailProcessingService(IResend resend, ILogger<EmailProcessingService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Initializing EmailProcessingService with queue: {QueueName}", QUEUE_NAME);
        _queueClient = new QueueClient(Environment.GetEnvironmentVariable("BlobStorage"), QUEUE_NAME);
        _resend = resend;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting EmailProcessingService background task");

        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _queueClient.ReceiveMessagesAsync(maxMessages: 32, cancellationToken: stoppingToken);

            foreach (var message in messages.Value)
            {
                var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));
                _logger.LogInformation("EmailProcessingService: Decoded message: {DecodedMessage}", decodedString);

                var emailMessage = JsonSerializer.Deserialize<AutoPile.DOMAIN.Models.MessageQueue.EmailMessage>(decodedString, _jsonOptions);

                var resendMessage = new Resend.EmailMessage
                {
                    From = $"{emailMessage.MessageType}@autopile.store",
                    To = { emailMessage.To },
                    Subject = emailMessage.Subject,
                    HtmlBody = emailMessage.Body
                };

                await _resend.EmailSendAsync(resendMessage);
                _logger.LogInformation("EmailProcessingService: Successfully sent email to {Recipient}", emailMessage.To);

                await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                _logger.LogInformation("EmailProcessingService: Deleted processed message {MessageId} from {QueueName}",
                    message.MessageId, QUEUE_NAME);

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}