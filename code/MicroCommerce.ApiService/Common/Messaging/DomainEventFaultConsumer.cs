using MassTransit;

namespace MicroCommerce.ApiService.Common.Messaging;

/// <summary>
/// Generic fault consumer that logs warnings when any domain event message faults.
/// MassTransit auto-discovers this via AddConsumers(assembly) since it implements IConsumer.
/// </summary>
/// <typeparam name="T">The faulted message type.</typeparam>
public sealed class DomainEventFaultConsumer<T> : IConsumer<Fault<T>> where T : class
{
    private readonly ILogger<DomainEventFaultConsumer<T>> _logger;

    public DomainEventFaultConsumer(ILogger<DomainEventFaultConsumer<T>> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Fault<T>> context)
    {
        var fault = context.Message;
        var messageType = typeof(T).Name;
        var messageId = fault.FaultedMessageId;
        var correlationId = context.CorrelationId?.ToString() ?? "none";
        var exceptions = string.Join("; ", fault.Exceptions.Select(e => e.Message));

        _logger.LogWarning(
            "Message faulted (DLQ entry). MessageType={MessageType}, MessageId={MessageId}, " +
            "CorrelationId={CorrelationId}, Exceptions={Exceptions}",
            messageType, messageId, correlationId, exceptions);

        return Task.CompletedTask;
    }
}
