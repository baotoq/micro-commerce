using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;

namespace MicroCommerce.Catalog.FunctionalTests.Products;

public class ProductsReadTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly HttpClient _seller = factory.CreateSellerClient();

    private static string NewSku() => $"FN-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    [Fact]
    public async Task GetProducts_Returns200WithPagedResult()
    {
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.GetAsync("/api/products", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResult<ProductDto>? result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>(ct);
        Assert.NotNull(result);
        Assert.True(result.Total >= 0);
    }

    [Fact]
    public async Task GetProductBySku_ExistingSku_Returns200()
    {
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Lookup Test", "Tableware", 25m, 3, "draft"), ct);

        HttpResponseMessage response = await _client.GetAsync($"/api/products/{sku}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ProductDto? product = await response.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(product);
        Assert.Equal(sku.ToUpperInvariant(), product.Sku);
    }

    [Fact]
    public async Task GetProductBySku_UnknownSku_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.GetAsync("/api/products/NONEXISTENT-SKU", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductCounts_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.GetAsync("/api/products/counts", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ProductCountsDto? counts = await response.Content.ReadFromJsonAsync<ProductCountsDto>(ct);
        Assert.NotNull(counts);
        Assert.True(counts.Total >= 0);
    }

    [Fact]
    public async Task GetProducts_StatusFilter_ReturnsOnlyMatchingStatus()
    {
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Filter Test", "Drinkware", 10m, 5, "active", PhotoUrls: ["https://example.com/p.jpg"]), ct);

        HttpResponseMessage response = await _client.GetAsync("/api/products?status=active&limit=100", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResult<ProductDto>? result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>(ct);
        Assert.NotNull(result);
        Assert.All(result.Items, p => Assert.Equal("active", p.Status));
    }

    [Fact]
    public async Task GetProducts_SearchByName_ReturnsMatchingProducts()
    {
        var sku = NewSku();
        string uniqueName = $"SearchableWidget-{Guid.NewGuid():N}"[..30];
        var ct = TestContext.Current.CancellationToken;
        await _seller.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, uniqueName, "Electronics", 9.99m, 5, "active", PhotoUrls: ["https://example.com/p.jpg"]), ct);

        HttpResponseMessage response = await _client.GetAsync($"/api/products?search={uniqueName[..10]}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedResult<ProductDto>? result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>(ct);
        Assert.NotNull(result);
        Assert.Contains(result.Items, p => string.Equals(p.Sku, sku, StringComparison.OrdinalIgnoreCase));
    }
}
