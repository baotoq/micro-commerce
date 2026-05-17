using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Queries;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Queries;

public class GetProductBySkuHandlerTests
{
    [Fact]
    public async Task Handle_ExistingSku_ReturnsDto()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductBySkuHandler(db);
        var result = await handler.Handle(new GetProductBySkuQuery("MC-001"), TestContext.Current.CancellationToken);

        Assert.Equivalent(new ProductDto("MC-001", "Widget", "Electronics", 9.99m, 10, "active", 0), result);
    }

    [Fact]
    public async Task Handle_NonExistentSku_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetProductBySkuHandler(db);

        var result = await handler.Handle(new GetProductBySkuQuery("MC-999"), TestContext.Current.CancellationToken);

        Assert.Null(result);
    }
}
