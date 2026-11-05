using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MicroCommerce.E2ETests;

/// <summary>
/// Arranges Catalog data through the published API, so the browser only acts and checks (research R8).
/// Request shapes follow the integration tests' CatalogSmokeTests. Tokens are never put in messages.
/// </summary>
public static class CatalogArrange
{
    public sealed record OpenedMerchant(Guid MerchantId, string ShopName);

    public sealed record CreatedProduct(Guid Id, int Version, string Name);

    public sealed record VariantInput(string[] OptionValues, long Price, int Stock, int WeightGrams = 400);

    public sealed record OptionInput(string Name, string[] Values);

    private sealed record Node(Guid Id, string Name, List<Node> Children);

    private sealed record Versioned(Guid Id, int Version);

    private sealed record Merchant(Guid MerchantId);

    public static async Task<OpenedMerchant> OpenMerchantAsync(this MarketplaceFixture fixture, string token, CancellationToken ct)
    {
        var shopName = TestRun.Unique("E2E Shop");
        var response = await SendAsync(fixture, HttpMethod.Post, "/api/me/merchant", token, new
        {
            shopName,
            description = "Created by the e2e suite",
            pickupAddress = new { street = "12 Le Loi", district = "District 1", province = "Ho Chi Minh City" },
        }, HttpStatusCode.Created, ct);
        var merchant = (await response.Content.ReadFromJsonAsync<Merchant>(JsonSerializerOptions.Web, ct))!;
        return new OpenedMerchant(merchant.MerchantId, shopName);
    }

    /// <summary>The starter tree's <c>Home › Kitchen</c> leaf.</summary>
    public static async Task<Guid> KitchenCategoryIdAsync(this MarketplaceFixture fixture, CancellationToken ct)
    {
        var tree = (await fixture.ApiClient.GetFromJsonAsync<List<Node>>("/api/marketplace/categories", ct))!;
        return tree.Single(c => c.Name == "Home").Children.Single(c => c.Name == "Kitchen").Id;
    }

    public static async Task<CreatedProduct> CreateProductAsync(
        this MarketplaceFixture fixture,
        string token,
        string name,
        OptionInput[]? options,
        VariantInput[]? variants,
        CancellationToken ct)
    {
        var response = await SendAsync(fixture, HttpMethod.Post, "/api/merchant/products", token, new
        {
            name,
            description = "Created by the e2e suite",
            categoryId = await fixture.KitchenCategoryIdAsync(ct),
            options = options ?? [],
            variants = variants ?? [new VariantInput([], 120_000, 5)],
        }, HttpStatusCode.Created, ct);
        var created = (await response.Content.ReadFromJsonAsync<Versioned>(JsonSerializerOptions.Web, ct))!;
        return new CreatedProduct(created.Id, created.Version, name);
    }

    public static async Task<CreatedProduct> UploadImageAsync(this MarketplaceFixture fixture, string token, CreatedProduct product, byte[] png, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/merchant/products/{product.Id}/images");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var file = new ByteArrayContent(png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        request.Content = new MultipartFormDataContent { { file, "file", "e2e.png" } };

        var response = await fixture.ApiClient.SendAsync(request, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(ct));
        var updated = (await response.Content.ReadFromJsonAsync<Versioned>(JsonSerializerOptions.Web, ct))!;
        return product with { Version = updated.Version };
    }

    public static async Task<CreatedProduct> PublishAsync(this MarketplaceFixture fixture, string token, CreatedProduct product, CancellationToken ct)
    {
        var response = await SendAsync(fixture, HttpMethod.Post, $"/api/merchant/products/{product.Id}/publish", token,
            new { version = product.Version }, HttpStatusCode.OK, ct);
        var updated = (await response.Content.ReadFromJsonAsync<Versioned>(JsonSerializerOptions.Web, ct))!;
        return product with { Version = updated.Version };
    }

    /// <summary>A Platform Operator removes the Product from the Marketplace.</summary>
    public static Task RemoveAsync(this MarketplaceFixture fixture, string operatorToken, CreatedProduct product, CancellationToken ct) =>
        SendAsync(fixture, HttpMethod.Post, $"/api/ops/products/{product.Id}/remove", operatorToken, null, HttpStatusCode.OK, ct);

    private static async Task<HttpResponseMessage> SendAsync(
        MarketplaceFixture fixture,
        HttpMethod method,
        string url,
        string token,
        object? body,
        HttpStatusCode expected,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body, options: JsonSerializerOptions.Web);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await fixture.ApiClient.SendAsync(request, ct);
        response.StatusCode.ShouldBe(expected, $"{method} {url}: {await response.Content.ReadAsStringAsync(ct)}");
        return response;
    }
}
