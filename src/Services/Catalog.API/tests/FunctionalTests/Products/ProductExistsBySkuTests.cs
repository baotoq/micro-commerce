using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Products.Commands;

namespace MicroCommerce.Catalog.FunctionalTests.Products;

public class ProductExistsBySkuTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string NewSku() => $"FN-EX-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    [Fact]
    public async Task ProductExistsBySku_ExistingSku_Returns204()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Exists Test", "Vessels", 10m, 1, "draft"), ct);

        var response = await _client.GetAsync($"/api/products/by-sku/{sku}/exists", ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ProductExistsBySku_MissingSku_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();

        var response = await _client.GetAsync($"/api/products/by-sku/{sku}/exists", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ProductExistsBySku_LowercaseSku_NormalizedAndMatches()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Case Test", "Vessels", 10m, 1, "draft"), ct);

        var response = await _client.GetAsync($"/api/products/by-sku/{sku.ToLowerInvariant()}/exists", ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ProductExistsBySku_AliasShortPath_AlsoWorks()
    {
        // AC-17 literal path: /api/products/{sku}/exists.
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Alias Test", "Vessels", 10m, 1, "draft"), ct);

        var response = await _client.GetAsync($"/api/products/{sku}/exists", ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
