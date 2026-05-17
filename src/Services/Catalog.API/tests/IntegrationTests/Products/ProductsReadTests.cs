using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.IntegrationTests;

namespace MicroCommerce.Catalog.IntegrationTests.Products;

[Collection("Api")]
public class ProductsReadTests(ApiFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task GetProducts_Returns200WithPagedResult()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
        Assert.NotNull(result);
        Assert.True(result.Total >= 0);
    }

    [Fact]
    public async Task GetProductBySku_UnknownSku_Returns404()
    {
        var response = await _client.GetAsync("/api/products/NONEXISTENT-SKU-999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductCounts_Returns200()
    {
        var response = await _client.GetAsync("/api/products/counts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var counts = await response.Content.ReadFromJsonAsync<ProductCountsDto>();
        Assert.NotNull(counts);
        Assert.True(counts.Total >= 0);
    }

    [Fact]
    public async Task GetProducts_StatusFilter_ReturnsOnlyMatchingStatus()
    {
        var sku = $"INT-{Guid.NewGuid():N}"[..16];
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Filter Test", "Drinkware", 10m, 5, "active"));

        var response = await _client.GetAsync("/api/products?status=active&limit=100");
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();

        Assert.NotNull(result);
        Assert.All(result.Items, p => Assert.Equal("active", p.Status));
    }
}
