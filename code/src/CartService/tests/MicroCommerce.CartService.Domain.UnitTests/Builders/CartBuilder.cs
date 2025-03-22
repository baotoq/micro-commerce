using MicroCommerce.CartService.Domain.Entities;
using MicroCommerce.CartService.Domain.ValueObjects;

namespace MicroCommerce.CartService.Domain.UnitTests.Builders;

public class CartBuilder
{
    private readonly Cart _cart;

    public CartBuilder()
    {
        _cart = new Cart(CartId.New());
    }

    public CartBuilder WithItem(Guid productId, int quantity, Price price)
    {
        _cart.AddItem(productId, quantity, price);
        return this;
    }

    public Cart Build()
    {
        return _cart;
    }
}
