using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Events;
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

        var handler = new UpdateProductHandler(db, new FakePublisher());
        var result = await handler.Handle(new UpdateProductCommand("MC-001", "Updated", "Gadgets", 19.99m, 5, "low"), TestContext.Current.CancellationToken);

        Assert.Equivalent(new ProductDto("MC-001", "Updated", "Gadgets", 19.99m, 5, "low", 0), result);
    }

    [Fact]
    public async Task Handle_ExistingProduct_PublishesProductUpdatedEvent()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var publisher = new FakePublisher();
        var handler = new UpdateProductHandler(db, publisher);
        await handler.Handle(new UpdateProductCommand("MC-001", "Updated", "Gadgets", 19.99m, 5, "low"), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<ProductUpdatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("MC-001", evt.Sku);
        Assert.Equal("Updated", evt.Name);
        Assert.Equal("Gadgets", evt.Category);
        Assert.Equal(19.99m, evt.Price);
        Assert.Equal(5, evt.Inventory);
        Assert.Equal("low", evt.Status);
    }

    [Fact]
    public async Task Handle_NonExistentSku_ReturnsNull()
    {
        await using var db = DbContextFactory.Create();
        var handler = new UpdateProductHandler(db, new FakePublisher());

        var result = await handler.Handle(new UpdateProductCommand("MC-999", "X", "Y", 1m, 1, "active"), TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_NonExistentSku_DoesNotPublishEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new UpdateProductHandler(db, publisher);

        await handler.Handle(new UpdateProductCommand("MC-999", "X", "Y", 1m, 1, "active"), TestContext.Current.CancellationToken);

        Assert.Empty(publisher.Published);
    }
}
