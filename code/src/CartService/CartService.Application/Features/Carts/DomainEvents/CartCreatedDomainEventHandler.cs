using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.BuildingBlocks.Common.Events;
using MicroCommerce.CartService.Domain.Carts.DomainEvents;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.CartService.Application.Features.Carts.DomainEvents;

public class CartCreatedDomainEventHandler(ILogger<CartCreatedDomainEventHandler> logger) : IDomainEventHandler<CartCreatedDomainEvent>
{
    public Task Handle(CartCreatedDomainEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cart created with ID: {CartId}", @event.CartId);
        return Task.CompletedTask;
    }
}
