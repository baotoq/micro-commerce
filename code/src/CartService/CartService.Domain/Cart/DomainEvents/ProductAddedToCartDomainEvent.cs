using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.CartService.Domain.Cart.DomainEvents;

public sealed record ProductAddedToCartDomainEvent(CartId CartId, Guid ProductId, int Quantity) : DomainEvent
{
    public CartId CartId { get; } = CartId ?? throw new ArgumentNullException(nameof(CartId));
    public Guid ProductId { get; } = ProductId;
    public int Quantity { get; } = Quantity;
}
