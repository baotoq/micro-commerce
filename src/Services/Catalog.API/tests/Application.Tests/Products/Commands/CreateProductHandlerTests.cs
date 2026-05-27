using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Products.Events;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class CreateProductHandlerTests
{
    private static CreateProductCommand DraftCmd(string sku = "MC-001", string name = "Widget", string category = "Electronics") =>
        new(sku, name, category, 9.99m, 10, "draft");

    [Fact]
    public async Task Handle_ValidCommand_CreatesProductAndReturnsDto()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var result = await handler.Handle(DraftCmd(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("MC-001", result.Value!.Sku);
        Assert.Equal("Widget", result.Value.Name);
        Assert.Equal("draft", result.Value.Status);
        Assert.Single(db.Products);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesProductCreatedEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreateProductHandler(db, publisher);

        await handler.Handle(DraftCmd(), TestContext.Current.CancellationToken);

        var evt = Assert.IsType<ProductCreatedEvent>(Assert.Single(publisher.Published));
        Assert.Equal("MC-001", evt.Sku);
        Assert.Equal("Widget", evt.Name);
        Assert.Equal("Electronics", evt.Category);
        Assert.Equal(9.99m, evt.Price);
        Assert.Equal(10, evt.Inventory);
        Assert.Equal("draft", evt.Status);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ReturnsConflictAndDoesNotPublishEvent()
    {
        await using var db = DbContextFactory.Create();
        var publisher = new FakePublisher();
        var handler = new CreateProductHandler(db, publisher);
        await handler.Handle(DraftCmd(), TestContext.Current.CancellationToken);
        publisher.Published.Clear();

        var result = await handler.Handle(DraftCmd(name: "Other"), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal("PRODUCT_SKU_DUPLICATE", result.Error.Value.Code);
        Assert.Empty(publisher.Published);
    }

    [Theory]
    [InlineData("low")]
    [InlineData("out")]
    [InlineData("draft")]
    [InlineData("DRAFT")]
    public async Task Handle_ValidStatus_CreatesProduct(string status)
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var result = await handler.Handle(
            new CreateProductCommand("MC-001", "Widget", "Cat", 1m, 1, status),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(status.ToLowerInvariant(), result.Value!.Status);
    }

    [Fact]
    public async Task Handle_ActiveStatusWithPhotos_CreatesProduct()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        var result = await handler.Handle(
            new CreateProductCommand(
                "MC-001", "Widget", "Cat", 1m, 1, "active",
                PhotoUrls: ["https://example.com/p1.jpg"]),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("active", result.Value!.Status);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ThrowsArgumentException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db, new FakePublisher());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new CreateProductCommand("MC-001", "Widget", "Cat", 1m, 1, "unknown"), TestContext.Current.CancellationToken));
    }
}
