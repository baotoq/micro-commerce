using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Domain.Carts.DomainEvents;
using MicroCommerce.CartService.Domain.Common;

namespace MicroCommerce.CartService.Domain.Carts;

public class Cart(CartId id) : BaseAggregateRoot<CartId>(id)
{
    private readonly List<CartItem> _items = [];

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public Money Total => _items.Aggregate(Money.Zero, (sum, item) => sum + item.SubTotal) - Discount;

    public Money Discount { get; private set; } = Money.Zero;

    public static Cart Create()
    {
        var cart = new Cart(CartId.New());
        cart.AddDomainEvent(new CartCreatedDomainEvent(cart.Id));
        return cart;
    }

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

    public void ApplyDiscount(Money discount)
    {
        if (discount.Amount < 0)
            throw new ArgumentException("Discount amount must be greater than zero.");

        Discount = discount;
    }

    public void RemoveItem(CartItemId cartItemId)
    {
        var item = _items.FirstOrDefault(i => i.CartItemId == cartItemId);
        if (item is null)
            throw new ArgumentException($"Item with ID {cartItemId} not found in cart.");

        _items.Remove(item);
        AddDomainEvent(new ProductRemovedFromCartDomainEvent(Id, cartItemId));
    }
}
