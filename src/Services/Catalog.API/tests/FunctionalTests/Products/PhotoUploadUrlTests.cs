using System.Net;
using System.Net.Http.Json;
using MicroCommerce.Catalog.Application.Products.Photos;

namespace MicroCommerce.Catalog.FunctionalTests.Products;

public class PhotoUploadUrlTests(CatalogWebApplicationFactory factory) : IClassFixture<CatalogWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/heic")]
    public async Task PhotoUploadUrl_AllowedContentType_Returns200WithUploadUrl(string contentType)
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync(
            "/api/products/photo-upload-url",
            new { contentType, sizeBytes = 1024 }, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PhotoUploadUrlResponse>(ct);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.UploadUrl));
        Assert.False(string.IsNullOrWhiteSpace(body.BlobUrl));
        Assert.Contains("photos/products/", body.BlobUrl, StringComparison.OrdinalIgnoreCase);
        Assert.True(body.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task PhotoUploadUrl_DisallowedContentType_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync(
            "/api/products/photo-upload-url",
            new { contentType = "application/pdf", sizeBytes = 1024 }, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task PhotoUploadUrl_OversizedFile_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;
        const long tooBig = 8L * 1024 * 1024 + 1;

        var response = await _client.PostAsJsonAsync(
            "/api/products/photo-upload-url",
            new { contentType = "image/jpeg", sizeBytes = tooBig }, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task PhotoUploadUrl_ZeroSize_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync(
            "/api/products/photo-upload-url",
            new { contentType = "image/jpeg", sizeBytes = 0 }, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PhotoUploadUrl_BlobNameUnderProductsFolder()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync(
            "/api/products/photo-upload-url",
            new { contentType = "image/jpeg", sizeBytes = 2048 }, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PhotoUploadUrlResponse>(ct);
        Assert.NotNull(body);
        // PRD §4: products/{guid}-...{ext}
        Assert.Matches(@"/photos/products/[^/]+\.jpg($|\?)", body!.UploadUrl);
    }
}
