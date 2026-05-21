using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;

namespace MicroCommerce.Catalog.FunctionalTests.Products;

public class ProductsWriteTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string NewSku() => $"FN-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    [Fact]
    public async Task CreateProduct_ValidRequest_Returns201WithLocation()
    {
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Functional Test Vase", "Vessels", 49.99m, 10, "active"), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        ProductDto? created = await response.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(created);
        Assert.Equal(sku.ToUpperInvariant(), created.Sku);
        Assert.Equal("active", created.Status);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_Returns409()
    {
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;
        var command = new CreateProductCommand(sku, "Original", "Vessels", 49.99m, 10, "active");
        await _client.PostAsJsonAsync("/api/products", command, ct);

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products", command with { Name = "Duplicate" }, ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_KnownSku_Returns200WithUpdatedData()
    {
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Before Update", "Drinkware", 20m, 5, "draft"), ct);

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/products/{sku}",
            new UpdateProductCommand(sku, "After Update", "Tableware", 35m, 8, "active"), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ProductDto? updated = await response.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(updated);
        Assert.Equal("After Update", updated.Name);
        Assert.Equal("active", updated.Status);
        Assert.Equal(35m, updated.Price);
    }

    [Fact]
    public async Task UpdateProduct_UnknownSku_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/products/NOSUCH-SKU-999",
            new UpdateProductCommand("NOSUCH-SKU-999", "X", "Y", 1m, 1, "draft"), ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_KnownSku_Returns204AndRemoves()
    {
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;
        await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "To Delete", "Vessels", 15m, 1, "draft"), ct);

        HttpResponseMessage deleteResponse = await _client.DeleteAsync($"/api/products/{sku}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage getResponse = await _client.GetAsync($"/api/products/{sku}", ct);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_UnknownSku_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.DeleteAsync("/api/products/NOSUCH-SKU-888", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ConcurrentSameSku_ReturnsExactlyOneConflict()
    {
        // TOCTOU regression guard (audit#3). Pre-check + SaveChangesAsync without a transaction
        // means two concurrent requests can both pass the AnyAsync check and one would otherwise
        // bubble a PostgresException(23505) as 500. The handler must catch DbUpdateException and
        // surface a Conflict result so exactly one request wins.
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;
        var command = new CreateProductCommand(sku, "Race", "Vessels", 1m, 1, "active");

        var tasks = Enumerable.Range(0, 8)
            .Select(_ => _client.PostAsJsonAsync("/api/products", command, ct))
            .ToArray();
        var responses = await Task.WhenAll(tasks);

        var created = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflict = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
        Assert.Equal(1, created);
        Assert.Equal(responses.Length - 1, conflict);
    }

    [Fact]
    public async Task CreateProduct_NegativePrice_Returns400ProblemDetails()
    {
        // Global ProblemDetails mapper test (audit#7). Domain throws
        // ArgumentOutOfRangeException for negative price and the mapper translates it
        // into 400 application/problem+json instead of a 500.
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "Bad", "Vessels", -1m, 1, "active"), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task CreateProduct_EmptyName_Returns400ProblemDetails()
    {
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "", "Vessels", 1m, 1, "active"), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task CreateProduct_InvalidStatus_Returns400ProblemDetails()
    {
        var sku = NewSku();
        var ct = TestContext.Current.CancellationToken;

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/products",
            new CreateProductCommand(sku, "X", "Vessels", 1m, 1, "bogus"), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }
}
