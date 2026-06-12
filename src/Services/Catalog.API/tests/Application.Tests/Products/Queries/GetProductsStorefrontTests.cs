using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Queries;
using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests.Products.Queries;

public class GetProductsStorefrontTests
{
    private static Product P(string sku, string name, string category, decimal price, ProductStatus status) =>
        TestProducts.Create(sku, name, category, price, 10, status);

    private static async Task<AppDbContext> SeedAsync()
    {
        var db = DbContextFactory.Create();
        db.AddRange(
            P("MC-A", "Active vase", "Vessels", 86m, ProductStatus.Active),
            P("MC-B", "Low bowl", "Tableware", 30m, ProductStatus.Low),
            P("MC-C", "Out mug", "Drinkware", 20m, ProductStatus.Out),
            P("MC-D", "Draft pot", "Vessels", 50m, ProductStatus.Draft));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return db;
    }

    [Fact]
    public async Task Buyable_ExcludesDraftAndOut()
    {
        await using var db = await SeedAsync();
        var handler = new GetProductsHandler(db);
        var page = await handler.Handle(
            new GetProductsQuery(1, 20, null, null, Buyable: true), TestContext.Current.CancellationToken);
        Assert.Equal(["MC-A", "MC-B"], page.Items.Select(i => i.Sku).Order().ToArray());
    }

    [Fact]
    public async Task Category_Filters()
    {
        await using var db = await SeedAsync();
        var handler = new GetProductsHandler(db);
        var page = await handler.Handle(
            new GetProductsQuery(1, 20, null, null, Buyable: true, Category: "Vessels"),
            TestContext.Current.CancellationToken);
        Assert.Equal(["MC-A"], page.Items.Select(i => i.Sku).ToArray());
    }

    [Fact]
    public async Task Sort_PriceAscAndDesc()
    {
        await using var db = await SeedAsync();
        var handler = new GetProductsHandler(db);
        var asc = await handler.Handle(
            new GetProductsQuery(1, 20, null, null, Buyable: true, Sort: "price-asc"),
            TestContext.Current.CancellationToken);
        var desc = await handler.Handle(
            new GetProductsQuery(1, 20, null, null, Buyable: true, Sort: "price-desc"),
            TestContext.Current.CancellationToken);
        Assert.Equal(["MC-B", "MC-A"], asc.Items.Select(i => i.Sku).ToArray());
        Assert.Equal(["MC-A", "MC-B"], desc.Items.Select(i => i.Sku).ToArray());
    }
}
