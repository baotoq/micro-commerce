using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.CartService.Domain.Carts.DomainEvents;

public sealed record ProductAddedToCartDomainEvent(CartId CartId, CartItem CartItem) : DomainEvent
{
}
