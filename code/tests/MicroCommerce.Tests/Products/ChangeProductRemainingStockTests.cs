using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.Products;
using Microsoft.EntityFrameworkCore;
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

        await SeedContext.Products.AddAsync(product);
        await SeedContext.SaveChangesAsync();

        var sut = new ChangeProductRemainingStock.Handler(SeedContext, NullLogger<ChangeProductRemainingStock.Handler>.Instance);

        var act = await sut.Handle(new ChangeProductRemainingStock.Command
        {
            ProductId = product.Id,
            ChangeQuantity = 100
        }, default);

        var updatedProduct = await VerifyContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == act.ProductId);

        await Verify(updatedProduct);
    }
}
