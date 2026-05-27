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
            TestProducts.Create("MC-001", "A", status: ProductStatus.Active),
            TestProducts.Create("MC-002", "B", status: ProductStatus.Active),
            TestProducts.Create("MC-003", "C", status: ProductStatus.Low),
            TestProducts.Create("MC-004", "D", status: ProductStatus.Out),
            TestProducts.Create("MC-005", "E", status: ProductStatus.Draft)
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
