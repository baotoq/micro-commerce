using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.CartService.Domain.Carts.DomainEvents;

public sealed record ProductAddedToCartDomainEvent(CartId CartId, CartItem CartItem) : DomainEvent
{
}
