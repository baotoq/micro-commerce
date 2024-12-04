using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.Tests.Carts;

public class RemoveProductToCartTests : TestBase
{
    [Fact]
    public async Task Success()
    {
        // Arrange
        var product = new Product
        {
            Name = "Product 1",
            Price = 100,
            RemainingStock = 10,
            TotalStock = 10
        };
        var cart = new Cart
        {
            Status = CartStatus.Pending,
            CartItems = new List<CartItem>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductQuantity = 1
                },
                new()
                {
                    Product = new Product
                    {
                        Name = "Product 2",
                        Price = 100,
                        RemainingStock = 10,
                        TotalStock = 10
                    },
                    ProductQuantity = 5
                }
            }
        };

        await SeedContext.AddAsync(product);
        await SeedContext.AddAsync(cart);
        await SeedContext.SaveChangesAsync();

        var distributedLockFactory = TestHelper.CreateAcquiredLock();

        var sut = new RemoveProductToCart.Handler(SeedContext, NullLogger<RemoveProductToCart.Handler>.Instance, distributedLockFactory);

        // Act
        var act = await sut.Handle(new RemoveProductToCart.Command
        {
            ProductId = product.Id,
            Quantity = 1,
            CartId = cart.Id
        }, default);

        cart = await VerifyContext.Carts
            .Include(s => s.CartItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == act.CartId);

        // Assert
        await Verify(cart, VerifySettings);
    }
}
