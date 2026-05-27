using MicroCommerce.Catalog.Domain.Products;

namespace MicroCommerce.Catalog.Application.Tests;

/// <summary>
/// Test factory: Active products require photos by domain invariant. Centralising the
/// "give me a valid Product for a status" plumbing keeps unrelated query/count tests
/// from drowning in photo-url boilerplate.
/// </summary>
internal static class TestProducts
{
    private static readonly string[] OnePhoto = ["https://example.com/p.jpg"];

    public static Product Create(
        string sku, string name = "Widget", string category = "Cat",
        decimal price = 1m, int inventory = 1,
        ProductStatus status = ProductStatus.Draft) =>
        new(
            Sku.From(sku), name, category, price, inventory, status,
            description: null,
            tags: [],
            weight: 0.5m,
            origin: "Portland, OR",
            photoUrls: status == ProductStatus.Active ? OnePhoto : []);
}
