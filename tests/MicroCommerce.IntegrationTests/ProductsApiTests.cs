using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MicroCommerce.IntegrationTests;

public class ProductsApiTests(AppHostFixture fixture)
{
    private sealed record CategoryNode(Guid Id, string Name, List<CategoryNode> Children);

    private sealed record Variant(Guid Id, long Price, int Stock, int WeightGrams);

    private sealed record Product(Guid Id, string Name, Guid CategoryId, string Status, List<Variant> Variants);

    private sealed record ProductSummary(Guid Id, string Name, string Status, long MinPrice, long MaxPrice, long TotalStock);

    private sealed record Merchant(Guid Id);

    private sealed record StorefrontProduct(Guid Id, string Name, long MinPrice, long MaxPrice, bool InStock, Guid ShopId, string ShopName);

    private sealed record StorefrontPage(List<StorefrontProduct> Items, int TotalCount);

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);

    private readonly HttpClient _client = fixture.ApiClient;

    [Fact]
    public async Task Health_endpoint_is_healthy()
    {
        var response = await _client.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Seeded_merchant_opens_a_shop_and_publishes_a_product_that_buyers_find()
    {
        var ct = TestContext.Current.CancellationToken;
        var token = await GetTokenAsync("merchant", "merchant", ct);
        var tree = await _client.GetFromJsonAsync<List<CategoryNode>>("/storefront/categories", ct);
        var kitchen = tree!.Single(n => n.Name == "Home").Children.Single(n => n.Name == "Kitchen");

        var shopName = $"Max's Mugs {Guid.NewGuid():N}";
        var opened = await SendAsync(HttpMethod.Post, "/me/merchant", token, new
        {
            ShopName = shopName,
            PickupAddress = new { Street = "12 Le Loi", Ward = "Ben Nghe", District = "District 1", Province = "Ho Chi Minh City" },
        }, ct);
        opened.StatusCode.ShouldBe(HttpStatusCode.Created);
        var merchant = (await opened.Content.ReadFromJsonAsync<Merchant>(ct)).ShouldNotBeNull();

        var created = await SendAsync(HttpMethod.Post, "/merchant/products", token, new
        {
            Name = "Coffee Mug",
            Description = "Stoneware.",
            CategoryId = kitchen.Id,
            Variants = new[] { new { Price = 120_000, Stock = 5, WeightGrams = 350 } },
        }, ct);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var product = (await created.Content.ReadFromJsonAsync<Product>(ct)).ShouldNotBeNull();
        product.Status.ShouldBe("Draft");
        product.Variants.ShouldHaveSingleItem().Price.ShouldBe(120_000);

        var published = await SendAsync(HttpMethod.Post, $"/merchant/products/{product.Id}/publish", token, null, ct);
        published.StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await SendAsync(HttpMethod.Get, "/merchant/products", token, null, ct);
        var summaries = await list.Content.ReadFromJsonAsync<List<ProductSummary>>(ct);
        summaries.ShouldNotBeNull().ShouldContain(new ProductSummary(product.Id, "Coffee Mug", "Published", 120_000, 120_000, 5));

        // Anonymously, once the Shop has reached the Catalog through the outbox.
        var found = await EventuallyAsync(
            () => _client.GetFromJsonAsync<StorefrontPage>($"/storefront/products?shopId={merchant.Id}", ct),
            page => page!.TotalCount > 0,
            ct);
        found!.Items.ShouldBe([new StorefrontProduct(product.Id, "Coffee Mug", 120_000, 120_000, true, merchant.Id, shopName)]);
    }

    private static async Task<T> EventuallyAsync<T>(Func<Task<T>> read, Func<T, bool> until, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        while (true)
        {
            var value = await read();
            if (until(value)) return value;
            await Task.Delay(100, timeout.Token);
        }
    }

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string token, object? body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, path) { Content = body is null ? null : JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request, ct);
    }

    // Password grant through the realm's test-only client; the frontend client doesn't allow it.
    private async Task<string> GetTokenAsync(string username, string password, CancellationToken ct)
    {
        var response = await fixture.KeycloakClient.PostAsync(
            "/realms/micro-commerce/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "micro-commerce-tests",
                ["username"] = username,
                ["password"] = password,
                ["scope"] = "openid",
            }),
            ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(ct));
        return (await response.Content.ReadFromJsonAsync<TokenResponse>(ct))!.AccessToken;
    }
}
