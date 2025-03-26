using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Common;

namespace MicroCommerce.CartService.Domain.UnitTests.TestHelpers.Builders;

public class CartBuilder
{
    private readonly Cart _cart;

    public CartBuilder()
    {
        _cart = new Cart(CartId.New());
    }

    public CartBuilder WithItem(CartItemId cartItemId, int quantity, Money price)
    {
        _cart.AddItem(cartItemId, quantity, price);
        return this;
    }

    public Cart Build()
    {
        return _cart;
    }
}
