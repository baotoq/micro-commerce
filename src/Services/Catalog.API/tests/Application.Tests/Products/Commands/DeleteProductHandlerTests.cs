using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Events;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class DeleteProductHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProduct_RemovesAndReturnsTrue()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(TestProducts.Create("MC-001", "Widget", category: "Electronics", price: 9.99m, inventory: 10, status: ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new DeleteProductHandler(db, new FakePublisher());
        var result = await handler.Handle(new DeleteProductCommand("MC-001"), TestContext.Current.CancellationToken);

        Assert.True(result);
        Assert.Empty(db.Products);
    }

    [Fact]
    public async Task Handle_ExistingProduct_PublishesProductDeletedEvent()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(TestProducts.Create("MC-001", "Widget", category: "Electronics", price: 9.99m, inventory: 10, status: ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var publisher = new FakePublisher();
        var handler = new DeleteProductHandler(db, publisher);
        await handler.Handle(new DeleteProductCommand("MC-001"), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<ProductDeletedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("MC-001", evt.Sku);
    }

    [Fact]
    public async Task Handle_NonExistentSku_ReturnsFalse()
    {
        await using var db = DbContextFactory.Create();
        var handler = new DeleteProductHandler(db, new FakePublisher());

        var result = await handler.Handle(new DeleteProductCommand("MC-999"), TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task Handle_NonExistentSku_DoesNotPublishEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new DeleteProductHandler(db, publisher);

        await handler.Handle(new DeleteProductCommand("MC-999"), TestContext.Current.CancellationToken);

        Assert.Empty(publisher.Published);
    }
}
