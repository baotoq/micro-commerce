using Domain.Common;
using MassTransit;

namespace Api.UseCases.Carts.DomainEvents;

public record CartCheckedOutDomainEvent : DomainEventBase
{
    public required string CartId { get; init; }
}

public class SendCheckoutEmailAfterCartCheckedOutDomainEventConsumer : IConsumer<CartCheckedOutDomainEvent>
{
    private readonly ILogger<SendCheckoutEmailAfterCartCheckedOutDomainEventConsumer> _logger;

    public SendCheckoutEmailAfterCartCheckedOutDomainEventConsumer(ILogger<SendCheckoutEmailAfterCartCheckedOutDomainEventConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CartCheckedOutDomainEvent> context)
    {
        var cartId = context.Message.CartId;
        
        _logger.LogInformation("Checkout email sent successfully for cart {CartId}", cartId);
        
        return Task.CompletedTask;
    }
}