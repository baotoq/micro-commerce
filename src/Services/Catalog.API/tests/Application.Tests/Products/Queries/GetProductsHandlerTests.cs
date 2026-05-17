using MicroCommerce.Catalog.Application.Products.Queries;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Queries;

public class GetProductsHandlerTests
{
    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyPagedResult()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetProductsHandler(db);

        var result = await handler.Handle(new GetProductsQuery(1, 10, null, null), TestContext.Current.CancellationToken);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllProducts()
    {
        await using var db = DbContextFactory.Create();
        db.Products.AddRange(
            new Product(Sku.From("MC-001"), "A", "Cat", 1m, 1, ProductStatus.Active),
            new Product(Sku.From("MC-002"), "B", "Cat", 1m, 1, ProductStatus.Draft)
        );
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductsHandler(db);
        var result = await handler.Handle(new GetProductsQuery(1, 10, null, null), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    [Theory]
    [InlineData("active", ProductStatus.Active)]
    [InlineData("low", ProductStatus.Low)]
    [InlineData("out", ProductStatus.Out)]
    [InlineData("draft", ProductStatus.Draft)]
    public async Task Handle_StatusFilter_ReturnsOnlyMatchingProducts(string statusFilter, ProductStatus matchingStatus)
    {
        await using var db = DbContextFactory.Create();
        db.Products.AddRange(
            new Product(Sku.From("MC-001"), "A", "Cat", 1m, 1, matchingStatus),
            new Product(Sku.From("MC-002"), "B", "Cat", 1m, 1, matchingStatus),
            new Product(Sku.From("MC-003"), "C", "Cat", 1m, 1, ProductStatus.Draft == matchingStatus ? ProductStatus.Active : ProductStatus.Draft)
        );
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductsHandler(db);
        var result = await handler.Handle(new GetProductsQuery(1, 10, statusFilter, null), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Total);
        Assert.All(result.Items, item => Assert.Equal(statusFilter, item.Status));
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        await using var db = DbContextFactory.Create();
        db.Products.AddRange(Enumerable.Range(1, 5).Select(i =>
            new Product(Sku.From($"MC-{i:D3}"), $"Product {i}", "Cat", 1m, 1, ProductStatus.Active)));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductsHandler(db);
        var result = await handler.Handle(new GetProductsQuery(2, 2, null, null), TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task Handle_UnknownStatusFilter_ReturnsAllProducts()
    {
        await using var db = DbContextFactory.Create();
        db.Products.Add(new Product(Sku.From("MC-001"), "A", "Cat", 1m, 1, ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductsHandler(db);
        var result = await handler.Handle(new GetProductsQuery(1, 10, "bogus", null), TestContext.Current.CancellationToken);

        Assert.Equal(1, result.Total);
    }
}
