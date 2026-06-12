using MicroCommerce.Catalog.Application.Products.Queries;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Queries;

public class GetProductCategoriesTests
{
    [Fact]
    public async Task ReturnsDistinctBuyableCategories_Sorted()
    {
        await using var db = DbContextFactory.Create();
        db.AddRange(
            TestProducts.Create("MC-1", "A", "Vessels", 10m, 5, ProductStatus.Active),
            TestProducts.Create("MC-2", "B", "Tableware", 10m, 5, ProductStatus.Low),
            TestProducts.Create("MC-3", "C", "Vessels", 10m, 5, ProductStatus.Active),
            TestProducts.Create("MC-4", "D", "Hidden", 10m, 5, ProductStatus.Draft));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetProductCategoriesHandler(db);
        var categories = await handler.Handle(new GetProductCategoriesQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(["Tableware", "Vessels"], categories);
    }
}
