using System.Threading.Tasks;
using MicroCommerce.CartService.Domain.Carts;
using MicroCommerce.CartService.Domain.Common;
using MicroCommerce.CartService.Domain.UnitTests.TestHelpers.Builders;

namespace MicroCommerce.CartService.Domain.UnitTests.Carts;

public class CartTests
{
    [Fact]
    public async Task AddItem_ShouldAddNewItem_WhenProductDoesNotExist()
    {
        // Arrange
        var cart = new CartBuilder().Build();
        var productId = CartItemId.New();
        var quantity = 2;
        var price = new Money(10.0m);

        // Act
        cart.AddItem(productId, quantity, price);

        // Assert
        await Verify(cart.Items);
    }

    [Fact]
    public async Task AddItem_ShouldIncreaseQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var cart = new CartBuilder().WithItem(CartItemId.New(), 1, new Money(10.0m)).Build();
        var productId = CartItemId.New();
        var additionalQuantity = 2;
        var price = new Money(10.0m);

        // Act
        cart.AddItem(productId, additionalQuantity, price);

        // Assert
        await Verify(cart.Items);
    }

    [Fact]
    public async Task AddItem_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var cart = new CartBuilder().Build();
        var productId = CartItemId.New();
        var invalidQuantity = 0;
        var price = new Money(10.0m);

        // Act & Assert
        await Throws(() => cart.AddItem(productId, invalidQuantity, price));
    }

    [Fact]
    public async Task ApplyDiscount_ShouldThrowException_WhenDiscountAmountIsNegative()
    {
        // Arrange
        var cart = new CartBuilder().Build();
        var discount = new Money(-10.0m);

        // Act & Assert
        await Throws(() => cart.ApplyDiscount(discount));
    }

    [Fact]
    public async Task ApplyDiscount_ShouldApplyDiscount_WhenDiscountAmountIsPositive()
    {
        // Arrange
        var cart = new CartBuilder().Build();
        var discount = new Money(10.0m);

        // Act
        cart.ApplyDiscount(discount);

        // Assert
        await Verify(cart.Discount);
    }

    [Fact]
    public async Task Total_ShouldReturnZero_WhenCartIsEmpty()
    {
        // Arrange
        var cart = new CartBuilder().Build();

        // Act
        var total = cart.Total;

        // Assert
        await Verify(total);
    }

    [Fact]
    public async Task Total_ShouldReturnTotalAmount_WhenCartHasItems()
    {
        // Arrange
        var cart = new CartBuilder().WithItem(CartItemId.New(), 1, new Money(10.0m)).Build();

        // Act
        var total = cart.Total;

        // Assert
        await Verify(total);
    }
}
