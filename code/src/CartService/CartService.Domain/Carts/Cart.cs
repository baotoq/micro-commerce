using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Domain.Carts.DomainEvents;
using MicroCommerce.CartService.Domain.Common;

namespace MicroCommerce.CartService.Domain.Carts;

public class Cart(CartId id) : BaseAggregateRoot<CartId>(id)
{
    private readonly List<CartItem> _items = [];

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public void AddItem(Guid productId, int quantity, Money price)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new CartItem(productId, quantity, price));
        }

        AddDomainEvent(new ProductAddedToCartDomainEvent(Id, productId, quantity));
    }
}
