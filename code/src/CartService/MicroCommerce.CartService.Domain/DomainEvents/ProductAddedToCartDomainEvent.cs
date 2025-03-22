using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Domain.Entities;
using MicroCommerce.CartService.Domain.ValueObjects;

namespace MicroCommerce.CartService.Domain.DomainEvents;

public sealed record ProductAddedToCartDomainEvent(CartId cartId, Guid productId, int quantity) : DomainEvent
{
    public CartId CartId { get; } = cartId ?? throw new ArgumentNullException(nameof(cartId));
    public Guid ProductId { get; } = productId;
    public int Quantity { get; } = quantity;
}
