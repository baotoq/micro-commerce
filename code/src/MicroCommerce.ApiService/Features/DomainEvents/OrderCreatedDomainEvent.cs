using MassTransit;
using MicroCommerce.ApiService.Domain.Events;
using RedLockNet;

namespace MicroCommerce.ApiService.Features.DomainEvents;

public class OrderCreatedDomainEvent(Guid cartId) : IDomainEvent
{
    public Guid CartId { get; } = cartId;
}

public class OrderCreatedDomainEventConsumer(ILogger<OrderCreatedDomainEventConsumer> logger, IDistributedLockFactory distributedLockFactory) : IConsumer<OrderCreatedDomainEvent>
{
    private readonly IDistributedLockFactory _distributedLockFactory = distributedLockFactory;
    private readonly ILogger<OrderCreatedDomainEventConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderCreatedDomainEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("Order created for cart {CartId}", @event.CartId);

        await using var lockHandle = await _distributedLockFactory.CreateLockAsync(@event.CartId.ToString(), TimeSpan.FromSeconds(30));

        if (!lockHandle.IsAcquired)
        {
            _logger.LogWarning("Failed to acquire lock for cart {CartId}", @event.CartId);
            return;
        }

        _logger.LogInformation("Lock acquired for cart {CartId}", @event.CartId);

        // Process order

        _logger.LogInformation("Order processed for cart {CartId}", @event.CartId);
    }
}
