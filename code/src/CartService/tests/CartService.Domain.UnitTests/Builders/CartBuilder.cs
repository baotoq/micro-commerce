using MicroCommerce.CartService.Domain.Cart;
using MicroCommerce.CartService.Domain.Common;

namespace MicroCommerce.CartService.Domain.UnitTests.Builders;

public class CartBuilder
{
    private readonly Cart.Cart _cart;

    public CartBuilder()
    {
        _cart = new Cart.Cart(CartId.New());
    }

    public CartBuilder WithItem(Guid productId, int quantity, Price price)
    {
        _cart.AddItem(productId, quantity, price);
        return this;
    }

    public Cart.Cart Build()
    {
        return _cart;
    }
}
