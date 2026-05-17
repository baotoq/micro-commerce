using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;

namespace MicroCommerce.Catalog.Application.Tests.Products.Commands;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesProductAndReturnsDto()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db);

        var result = await handler.Handle(new CreateProductCommand("MC-001", "Widget", "Electronics", 9.99m, 10, "active"), TestContext.Current.CancellationToken);

        Assert.Equivalent(new ProductDto("MC-001", "Widget", "Electronics", 9.99m, 10, "active", 0), result);
        Assert.Single(db.Products);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsInvalidOperationException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db);
        await handler.Handle(new CreateProductCommand("MC-001", "Widget", "Electronics", 9.99m, 10, "active"), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CreateProductCommand("MC-001", "Other", "Electronics", 5m, 1, "active"), TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("active")]
    [InlineData("low")]
    [InlineData("out")]
    [InlineData("draft")]
    [InlineData("ACTIVE")]
    public async Task Handle_ValidStatus_CreatesProduct(string status)
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db);

        var result = await handler.Handle(new CreateProductCommand("MC-001", "Widget", "Cat", 1m, 1, status), TestContext.Current.CancellationToken);

        Assert.Equal(status.ToLowerInvariant(), result.Status);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ThrowsArgumentException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateProductHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(new CreateProductCommand("MC-001", "Widget", "Cat", 1m, 1, "unknown"), TestContext.Current.CancellationToken));
    }
}
