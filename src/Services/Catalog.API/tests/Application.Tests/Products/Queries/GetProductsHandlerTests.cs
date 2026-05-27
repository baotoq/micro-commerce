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
            TestProducts.Create("MC-001", "A", status: ProductStatus.Active),
            TestProducts.Create("MC-002", "B", status: ProductStatus.Draft)
        );
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductsHandler(db);
        var result = await handler.Handle(new GetProductsQuery(1, 10, null, null), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    public static TheoryData<string, ProductStatus, ProductStatus> StatusFilterCases() => new()
    {
        { "active", ProductStatus.Active, ProductStatus.Draft },
        { "low",    ProductStatus.Low,    ProductStatus.Draft },
        { "out",    ProductStatus.Out,    ProductStatus.Draft },
        { "draft",  ProductStatus.Draft,  ProductStatus.Low },
    };

    [Theory]
    [MemberData(nameof(StatusFilterCases))]
    public async Task Handle_StatusFilter_ReturnsOnlyMatchingProducts(string statusFilter, ProductStatus matchingStatus, ProductStatus nonMatchingStatus)
    {
        await using var db = DbContextFactory.Create();
        db.Products.AddRange(
            TestProducts.Create("MC-001", "A", status: matchingStatus),
            TestProducts.Create("MC-002", "B", status: matchingStatus),
            TestProducts.Create("MC-003", "C", status: nonMatchingStatus)
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
            TestProducts.Create($"MC-{i:D3}", $"Product {i}", status: ProductStatus.Active)));
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
        db.Products.Add(TestProducts.Create("MC-001", "A", status: ProductStatus.Active));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductsHandler(db);
        var result = await handler.Handle(new GetProductsQuery(1, 10, "bogus", null), TestContext.Current.CancellationToken);

        Assert.Equal(1, result.Total);
    }
}
