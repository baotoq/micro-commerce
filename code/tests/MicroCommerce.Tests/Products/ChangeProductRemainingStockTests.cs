using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.Products;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroCommerce.Tests.Products;

public class ChangeProductRemainingStockTests : TestBase
{
    [Fact]
    public async Task Success()
    {
        var product = new Product
        {
            Id = Guid.CreateVersion7(),
            Name = "Product 1",
            Price = 100,
            RemainingStock = 100,
            TotalStock = 100
        };

        await Context.Products.AddAsync(product);
        await Context.SaveChangesAsync();

        var sut = new ChangeProductRemainingStock.Handler(Context, NullLogger<ChangeProductRemainingStock.Handler>.Instance);

        var act = await sut.Handle(new ChangeProductRemainingStock.Command
        {
            ProductId = product.Id,
            ChangeQuantity = 100
        }, default);

        await Verify(act, VerifySettings);
    }
}
