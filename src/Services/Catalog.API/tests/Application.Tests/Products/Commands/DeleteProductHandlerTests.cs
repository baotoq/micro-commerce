using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class DeleteProductHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProduct_RemovesAndReturnsTrue()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteProductHandler(db);
        var result = await handler.Handle(new DeleteProductCommand("MC-001"), TestContext.Current.CancellationToken);

        Assert.True(result);
        Assert.Empty(db.Products);
    }

    [Fact]
    public async Task Handle_NonExistentSku_ReturnsFalse()
    {
        await using var db = DbContextFactory.Create();
        var handler = new DeleteProductHandler(db);

        var result = await handler.Handle(new DeleteProductCommand("MC-999"), TestContext.Current.CancellationToken);

        Assert.False(result);
    }
}
