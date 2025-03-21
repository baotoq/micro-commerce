using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Domain.DomainEvents;
using MicroCommerce.CartService.Domain.ValueObjects;

namespace MicroCommerce.CartService.Domain.Entities;

public class Cart : BaseEntity
{
    private readonly List<CartItem> _items = new();

    public CartId Id { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    public Cart(CartId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    public void AddItem(Guid productId, int quantity, decimal price)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(productId, quantity, price));
        }

        AddDomainEvent(new ProductAddedToCartDomainEvent(Id, productId, quantity));
        SetUpdated();
    }
}
