using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.IntegrationTests;

namespace MicroCommerce.Catalog.IntegrationTests.Products;

[Collection("Api")]
public class ProductsWriteTests(ApiFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    private static string NewSku() => $"INT-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    [Fact]
    public async Task CreateProduct_ValidRequest_Returns201WithLocation()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        var command = new CreateProductCommand(sku, "Integration Test Vase", "Vessels", 49.99m, 10, "active", PhotoUrls: ["https://example.com/p.jpg"]);

        var response = await _client.PostAsJsonAsync("/api/products", command, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(created);
        Assert.Equal(sku.ToUpperInvariant(), created.Sku);
        Assert.Equal("Integration Test Vase", created.Name);
        Assert.Equal("active", created.Status);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        var command = new CreateProductCommand(sku, "Original", "Vessels", 49.99m, 10, "active", PhotoUrls: ["https://example.com/p.jpg"]);
        await _client.PostAsJsonAsync("/api/products", command, ct);

        var response = await _client.PostAsJsonAsync("/api/products",
            command with { Name = "Duplicate" }, ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetProductBySku_AfterCreate_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Lookup Test", "Tableware", 25m, 3, "draft"), ct);

        var response = await _client.GetAsync($"/api/products/{sku}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(product);
        Assert.Equal(sku.ToUpperInvariant(), product.Sku);
    }

    [Fact]
    public async Task UpdateProduct_KnownSku_Returns200WithUpdatedData()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Before Update", "Drinkware", 20m, 5, "draft"), ct);

        var update = new UpdateProductCommand(sku, "After Update", "Tableware", 35m, 8, "active", PhotoUrls: ["https://example.com/p.jpg"]);
        var response = await _client.PutAsJsonAsync($"/api/products/{sku}", update, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(updated);
        Assert.Equal("After Update", updated.Name);
        Assert.Equal("active", updated.Status);
        Assert.Equal(35m, updated.Price);
    }

    [Fact]
    public async Task UpdateProduct_UnknownSku_Returns404()
    {
        var update = new UpdateProductCommand("NOSUCH-SKU-999", "X", "Y", 1m, 1, "draft");
        var response = await _client.PutAsJsonAsync("/api/products/NOSUCH-SKU-999", update, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_KnownSku_Returns204AndRemoves()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "To Delete", "Vessels", 15m, 1, "draft"), ct);

        var deleteResponse = await _client.DeleteAsync($"/api/products/{sku}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/products/{sku}", ct);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_UnknownSku_Returns404()
    {
        var response = await _client.DeleteAsync("/api/products/NOSUCH-SKU-888", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
