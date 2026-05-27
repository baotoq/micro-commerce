using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Products.Commands;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.IntegrationTests;

namespace MicroCommerce.Catalog.IntegrationTests.Products;

[Collection("Api")]
public class ProductsRichFieldsTests(ApiFixture fixture)
{
    private readonly HttpClient _client = fixture.HttpClient;

    private static string NewSku() => $"IT-RF-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

    [Fact]
    public async Task CreateProduct_WithAllRichFields_RoundTripsViaGet()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        var command = new CreateProductCommand(
            Sku: sku,
            Name: "Integration Rich Vase",
            Category: "Vessels",
            Price: 86m,
            Inventory: 10,
            Status: "active",
            Description: "A tall hand-thrown vase with a persimmon glaze.",
            Tags: ["ceramic", "minimal", "handmade"],
            Weight: 1.25m,
            Origin: "Portland, OR",
            PhotoUrls: ["https://example.com/photos/products/abc.jpg", "https://example.com/photos/products/def.jpg"]);

        var createResponse = await _client.PostAsJsonAsync("/api/products", command, ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(created);
        Assert.Equal("A tall hand-thrown vase with a persimmon glaze.", created.Description);
        Assert.Equal(new[] { "ceramic", "minimal", "handmade" }, created.Tags);
        Assert.Equal(1.25m, created.Weight);
        Assert.Equal("Portland, OR", created.Origin);
        Assert.Equal(
            new[] { "https://example.com/photos/products/abc.jpg", "https://example.com/photos/products/def.jpg" },
            created.PhotoUrls);

        var getResponse = await _client.GetAsync($"/api/products/{sku}", ct);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(fetched);
        Assert.Equal(created.Description, fetched.Description);
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
            Name: "Bad Active Product",
            Category: "Vessels",
            Price: 1m,
            Inventory: 1,
            Status: "active",
            PhotoUrls: []);

        var response = await _client.PostAsJsonAsync("/api/products", command, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task CreateProduct_NullableDescriptionOmitted_PersistsAsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        var sku = NewSku();
        var command = new CreateProductCommand(
            Sku: sku,
            Name: "No Description Vase",
            Category: "Vessels",
            Price: 20m,
            Inventory: 5,
            Status: "draft",
            Description: null,
            Tags: [],
            Weight: 0.5m,
            Origin: "Seattle, WA");

        var createResponse = await _client.PostAsJsonAsync("/api/products", command, ct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/products/{sku}", ct);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductDto>(ct);
        Assert.NotNull(fetched);
        Assert.Null(fetched.Description);
        Assert.Empty(fetched.Tags);
    }
}
