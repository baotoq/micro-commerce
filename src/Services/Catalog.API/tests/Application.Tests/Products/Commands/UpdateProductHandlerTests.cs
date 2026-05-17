using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class UpdateProductHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProduct_UpdatesAndReturnsDto()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(db);
        var result = await handler.Handle(new UpdateProductCommand("MC-001", "Updated", "Gadgets", 19.99m, 5, "low"), TestContext.Current.CancellationToken);

        Assert.Equivalent(new ProductDto("MC-001", "Updated", "Gadgets", 19.99m, 5, "low", 0), result);
    }

    [Fact]
    public async Task Handle_NonExistentSku_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new UpdateProductHandler(db);

        var result = await handler.Handle(new UpdateProductCommand("MC-999", "X", "Y", 1m, 1, "active"), TestContext.Current.CancellationToken);

        Assert.Null(result);
    }
}
