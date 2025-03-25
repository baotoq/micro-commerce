using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.CartService.Domain.Common;
using MicroCommerce.CartService.Domain.DomainEvents;

namespace MicroCommerce.CartService.Domain.Cart;

public class Cart(CartId id) : BaseAggregateRoot<CartId>(id)
{
    private readonly List<CartItem> _items = [];

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public void AddItem(Guid productId, int quantity, Price price)
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
    }
}
