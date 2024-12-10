using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.Tests.Carts;

public class PlaceOrderTests : TestBase
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
                }
            }
        };

        await SeedContext.AddAsync(product);
        await SeedContext.AddAsync(cart);
        await SeedContext.SaveChangesAsync();

        var distributedLockFactory = TestHelper.CreateAcquiredLock();

        var sut = new PlaceOrder.Handler(SeedContext, NullLogger<PlaceOrder.Handler>.Instance, distributedLockFactory);

        // Act
        var act = await sut.Handle(new PlaceOrder.Command { CartId = cart.Id }, default);

        cart = await VerifyContext.Carts
            .Include(s => s.CartItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == act.CartId);

        // Assert
        await Verify(cart);
    }
}
