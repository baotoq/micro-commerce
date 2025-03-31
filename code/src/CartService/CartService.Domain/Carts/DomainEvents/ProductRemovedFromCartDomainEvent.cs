using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.CartService.Domain.Carts.DomainEvents;

public record ProductRemovedFromCartDomainEvent(CartId CartId, CartItemId CartItemId) : DomainEvent;
