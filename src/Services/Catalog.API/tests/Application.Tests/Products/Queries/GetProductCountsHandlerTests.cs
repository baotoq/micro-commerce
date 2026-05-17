using MicroCommerce.Catalog.Application.Products.Queries;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Queries;

public class GetProductCountsHandlerTests
{
    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsAllZeros()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetProductCountsHandler(db);

        var result = await handler.Handle(new GetProductCountsQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(new(0, 0, 0, 0, 0), result);
    }

    [Fact]
    public async Task Handle_MixedStatuses_ReturnsCorrectCounts()
    {
        await using var db = DbContextFactory.Create();
        db.Products.AddRange(
            new Product(Sku.From("MC-001"), "A", "Cat", 1m, 1, ProductStatus.Active),
            new Product(Sku.From("MC-002"), "B", "Cat", 1m, 1, ProductStatus.Active),
            new Product(Sku.From("MC-003"), "C", "Cat", 1m, 1, ProductStatus.Low),
            new Product(Sku.From("MC-004"), "D", "Cat", 1m, 1, ProductStatus.Out),
            new Product(Sku.From("MC-005"), "E", "Cat", 1m, 1, ProductStatus.Draft)
        );
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductCountsHandler(db);
        var result = await handler.Handle(new GetProductCountsQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Active);
        Assert.Equal(1, result.Low);
        Assert.Equal(1, result.Out);
        Assert.Equal(1, result.Draft);
    }
}
