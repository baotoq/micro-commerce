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
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Draft));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(db, new FakePublisher());
        var result = await handler.Handle(new UpdateProductCommand("MC-001", "Updated", "Gadgets", 19.99m, 5, "low"), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("MC-001", result.Sku);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("Gadgets", result.Category);
        Assert.Equal(19.99m, result.Price);
        Assert.Equal(5, result.Inventory);
        Assert.Equal("low", result.Status);
    }

    [Fact]
    public async Task Handle_ExistingProduct_PublishesProductUpdatedEvent()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "Widget", "Electronics", 9.99m, 10, ProductStatus.Draft));
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

    // Regression: the legacy edit form omits photoUrls / description / tags on
    // submit. When the handler treated null as "clear", every active-status
    // save 400-errored on the photo invariant and wiped any stored
    // description/tags as a side effect.
    [Fact]
    public async Task Handle_OmittedNullableFields_PreserveStoredValues()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(
            Sku.From("MC-002"),
            "Persimmon vase",
            "Vessels",
            86m,
            10,
            ProductStatus.Active,
            description: "A hand-thrown stoneware vase.",
            tags: ["ceramic", "handmade"],
            weight: 1.25m,
            origin: "Portland, OR",
            photoUrls: ["https://blob/x.jpg"]));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateProductHandler(db, new FakePublisher());
        // Only the six fields the legacy edit form submits — description,
        // tags, and photoUrls are null on the wire.
        var result = await handler.Handle(
            new UpdateProductCommand(
                Sku: "MC-002",
                Name: "Edited Vase",
                Category: "Vessels",
                Price: 99m,
                Inventory: 5,
                Status: "active"),
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Edited Vase", result.Name);
        Assert.Equal(99m, result.Price);
        // Stored rich fields survived the partial update.
        Assert.Equal("A hand-thrown stoneware vase.", result.Description);
        Assert.Equal(new[] { "ceramic", "handmade" }, result.Tags);
        Assert.Equal(new[] { "https://blob/x.jpg" }, result.PhotoUrls);
    }
}
