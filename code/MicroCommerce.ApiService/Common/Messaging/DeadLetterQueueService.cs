using Azure.Messaging.ServiceBus;

namespace MicroCommerce.ApiService.Common.Messaging;

/// <summary>
/// DTO representing a dead-lettered message for admin visibility.
/// </summary>
public record DeadLetterMessageDto(
    long SequenceNumber,
    string MessageType,
    string ErrorDescription,
    string? CorrelationId,
    DateTimeOffset EnqueuedTime,
    string QueueName);

/// <summary>
/// Service for browsing, retrying, and purging dead-lettered messages
/// from Azure Service Bus DLQ sub-queues.
/// </summary>
public interface IDeadLetterQueueService
{
    Task<IReadOnlyList<DeadLetterMessageDto>> PeekDeadLettersAsync(
        string queueName, int maxMessages = 20, CancellationToken ct = default);

    Task RetryDeadLetterAsync(
        string queueName, long sequenceNumber, CancellationToken ct = default);

    Task<int> PurgeDeadLettersAsync(
        string queueName, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetQueueNamesAsync(CancellationToken ct = default);
}

public sealed class DeadLetterQueueService : IDeadLetterQueueService
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<DeadLetterQueueService> _logger;

    /// <summary>
    /// Known MassTransit consumer queue names.
    /// This list grows as more consumers are added in future phases.
    /// </summary>
    private static readonly string[] KnownQueueNames =
    [
        "product-created-domain-event"
    ];

    public DeadLetterQueueService(ServiceBusClient client, ILogger<DeadLetterQueueService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DeadLetterMessageDto>> PeekDeadLettersAsync(
        string queueName, int maxMessages = 20, CancellationToken ct = default)
    {
        await using var receiver = _client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter
        });

        try
        {
            var messages = await receiver.PeekMessagesAsync(maxMessages, cancellationToken: ct);

            return messages.Select(m => MapToDto(m, queueName)).ToList();
        }
        catch (ServiceBusException ex)
        {
            _logger.LogWarning(ex,
                "Failed to peek DLQ messages for queue {QueueName}. " +
                "This may be expected in emulator environments.", queueName);
            return [];
        }
    }

    public async Task RetryDeadLetterAsync(
        string queueName, long sequenceNumber, CancellationToken ct = default)
    {
        await using var dlqReceiver = _client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter
        });

        try
        {
            var message = await dlqReceiver.ReceiveMessagesAsync(32, cancellationToken: ct);
            var target = message.FirstOrDefault(m => m.SequenceNumber == sequenceNumber);

            if (target is null)
            {
                throw new InvalidOperationException(
                    $"Dead-letter message with sequence number {sequenceNumber} not found in queue {queueName}.");
            }

            // Re-send to original queue
            await using var sender = _client.CreateSender(queueName);
            var retryMessage = new ServiceBusMessage(target.Body)
            {
                ContentType = target.ContentType,
                Subject = target.Subject,
                CorrelationId = target.CorrelationId,
                MessageId = Guid.NewGuid().ToString()
            };

            // Copy application properties
            foreach (var prop in target.ApplicationProperties)
            {
                retryMessage.ApplicationProperties[prop.Key] = prop.Value;
            }

            await sender.SendMessageAsync(retryMessage, ct);

            // Complete the DLQ message to remove it
            await dlqReceiver.CompleteMessageAsync(target, ct);

            _logger.LogInformation(
                "Retried dead-letter message {SequenceNumber} from queue {QueueName}.",
                sequenceNumber, queueName);
        }
        catch (ServiceBusException ex)
        {
            _logger.LogWarning(ex,
                "Failed to retry DLQ message {SequenceNumber} for queue {QueueName}. " +
                "This may be expected in emulator environments.", sequenceNumber, queueName);
            throw;
        }
    }

    public async Task<int> PurgeDeadLettersAsync(string queueName, CancellationToken ct = default)
    {
        await using var receiver = _client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter
        });

        var purgedCount = 0;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var messages = await receiver.ReceiveMessagesAsync(32, TimeSpan.FromSeconds(5), ct);

                if (messages.Count == 0)
                    break;

                foreach (var message in messages)
                {
                    await receiver.CompleteMessageAsync(message, ct);
                    purgedCount++;
                }
            }

            _logger.LogInformation(
                "Purged {Count} dead-letter messages from queue {QueueName}.",
                purgedCount, queueName);
        }
        catch (ServiceBusException ex)
        {
            _logger.LogWarning(ex,
                "Failed to purge DLQ messages for queue {QueueName}. " +
                "Purged {Count} before failure. " +
                "This may be expected in emulator environments.", queueName, purgedCount);
        }

        return purgedCount;
    }

    public Task<IReadOnlyList<string>> GetQueueNamesAsync(CancellationToken ct = default)
    {
        IReadOnlyList<string> names = KnownQueueNames;
        return Task.FromResult(names);
    }

    private static DeadLetterMessageDto MapToDto(ServiceBusReceivedMessage message, string queueName)
    {
        var messageType = message.ApplicationProperties.TryGetValue("MT-MessageType", out var mtType)
            ? mtType?.ToString() ?? "Unknown"
            : "Unknown";

        var errorDescription = message.DeadLetterReason ?? "No reason provided";

        var correlationId = message.ApplicationProperties.TryGetValue("MT-Activity-CorrelationId", out var mtCorr)
            ? mtCorr?.ToString()
            : message.CorrelationId;

        return new DeadLetterMessageDto(
            message.SequenceNumber,
            messageType,
            errorDescription,
            correlationId,
            message.EnqueuedTime,
            queueName);
    }
}
