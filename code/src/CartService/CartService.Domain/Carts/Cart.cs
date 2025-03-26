using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Domain.Carts.DomainEvents;
using MicroCommerce.CartService.Domain.Common;

namespace MicroCommerce.CartService.Domain.Carts;

public class Cart(CartId id) : BaseAggregateRoot<CartId>(id)
{
    private readonly List<CartItem> _items = [];

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public void AddItem(CartItemId cartItemId, int quantity, Money price)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var existingItem = _items.FirstOrDefault(i => i.CartItemId == cartItemId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            existingItem = new CartItem(cartItemId, quantity, price);
            _items.Add(existingItem);
        }

        AddDomainEvent(new ProductAddedToCartDomainEvent(Id, existingItem));
    }
}
