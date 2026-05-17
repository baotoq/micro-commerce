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
        var sku = NewSku();
        var command = new CreateProductCommand(sku, "Integration Test Vase", "Vessels", 49.99m, 10, "active");

        var response = await _client.PostAsJsonAsync("/api/products", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.Equal(sku.ToUpperInvariant(), created.Sku);
        Assert.Equal("Integration Test Vase", created.Name);
        Assert.Equal("active", created.Status);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_Returns409()
    {
        var sku = NewSku();
        var command = new CreateProductCommand(sku, "Original", "Vessels", 49.99m, 10, "active");
        await _client.PostAsJsonAsync("/api/products", command);

        var response = await _client.PostAsJsonAsync("/api/products",
            command with { Name = "Duplicate" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetProductBySku_AfterCreate_Returns200()
    {
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Lookup Test", "Tableware", 25m, 3, "draft"));

        var response = await _client.GetAsync($"/api/products/{sku}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(product);
        Assert.Equal(sku.ToUpperInvariant(), product.Sku);
    }

    [Fact]
    public async Task UpdateProduct_KnownSku_Returns200WithUpdatedData()
    {
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Before Update", "Drinkware", 20m, 5, "draft"));

        var update = new UpdateProductCommand(sku, "After Update", "Tableware", 35m, 8, "active");
        var response = await _client.PutAsJsonAsync($"/api/products/{sku}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(updated);
        Assert.Equal("After Update", updated.Name);
        Assert.Equal("active", updated.Status);
        Assert.Equal(35m, updated.Price);
    }

    [Fact]
    public async Task UpdateProduct_UnknownSku_Returns404()
    {
        var update = new UpdateProductCommand("NOSUCH-SKU-999", "X", "Y", 1m, 1, "draft");
        var response = await _client.PutAsJsonAsync("/api/products/NOSUCH-SKU-999", update);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_KnownSku_Returns204AndRemoves()
    {
        var sku = NewSku();
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "To Delete", "Vessels", 15m, 1, "draft"));

        var deleteResponse = await _client.DeleteAsync($"/api/products/{sku}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/products/{sku}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_UnknownSku_Returns404()
    {
        var response = await _client.DeleteAsync("/api/products/NOSUCH-SKU-888");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
