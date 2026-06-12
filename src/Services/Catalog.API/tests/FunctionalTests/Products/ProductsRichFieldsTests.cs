using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;

namespace MicroCommerce.Catalog.FunctionalTests.Products;

public class ProductsRichFieldsTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly HttpClient _seller = factory.CreateSellerClient();

    private static string NewSku() => $"FN-RF-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    [Fact]
    public async Task CreateProduct_WithRichFields_PersistsAndReturnsThemViaGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        var command = new CreateProductCommand(
            Sku: sku,
            Name: "Rich field vase",
            Category: "Vessels",
            Price: 99m,
            Inventory: 5,
            Status: "active",
            Description: "An ornate vessel with a long body.",
            Tags: ["ceramic", "handmade"],
            Weight: 1.75m,
            Origin: "Portland, OR",
            PhotoUrls: ["https://example.com/p1.jpg", "https://example.com/p2.jpg"]);

        var createResponse = await _seller.PostAsJsonAsync("/api/products", command, ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(created);
        Assert.Equal("An ornate vessel with a long body.", created!.Description);
        Assert.Equal(new[] { "ceramic", "handmade" }, created.Tags);
        Assert.Equal(1.75m, created.Weight);
        Assert.Equal("Portland, OR", created.Origin);
        Assert.Equal(new[] { "https://example.com/p1.jpg", "https://example.com/p2.jpg" }, created.PhotoUrls);

        var getResponse = await _client.GetAsync($"/api/products/{sku}", ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(fetched);
        Assert.Equal(created.Description, fetched!.Description);
        Assert.Equal(created.Tags, fetched.Tags);
        Assert.Equal(created.Weight, fetched.Weight);
        Assert.Equal(created.Origin, fetched.Origin);
        Assert.Equal(created.PhotoUrls, fetched.PhotoUrls);
    }

    [Fact]
    public async Task CreateProduct_ActiveStatusWithNoPhotos_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        var command = new CreateProductCommand(
            Sku: sku,
            Name: "Bad active",
            Category: "Vessels",
            Price: 1m,
            Inventory: 1,
            Status: "active",
            PhotoUrls: []);

        var response = await _seller.PostAsJsonAsync("/api/products", command, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }
}
