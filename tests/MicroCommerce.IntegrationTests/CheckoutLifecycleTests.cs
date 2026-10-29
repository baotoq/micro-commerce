using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MicroCommerce.IntegrationTests;

public class CheckoutLifecycleTests(AppHostFixture fixture)
{
    private sealed record CategoryNode(Guid Id, string Name, List<CategoryNode> Children);
    private sealed record Variant(Guid Id);
    private sealed record Product(Guid Id, List<Variant> Variants);
    private sealed record Checkout(Guid Id, string Status, DateTimeOffset ExpiresAt);
    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);

    [Fact]
    public async Task Checkout_expires_on_its_payment_window_timer()
    {
        var ct = TestContext.Current.CancellationToken;
        var merchantToken = await GetTokenAsync("operator", "operator", ct);
        var buyerToken = await GetTokenAsync("buyer", "buyer", ct);
        var tree = await fixture.ApiClient.GetFromJsonAsync<List<CategoryNode>>("/storefront/categories", ct);
        var category = tree!.Single(c => c.Name == "Home").Children.Single(c => c.Name == "Kitchen");

        var merchant = await SendAsync(HttpMethod.Post, "/me/merchant", merchantToken, new
        {
            shopName = $"Timer Shop {Guid.NewGuid():N}",
            pickupAddress = new
            {
                street = "12 Le Loi",
                ward = "Ben Nghe",
                district = "District 1",
                province = "Hồ Chí Minh",
            },
        }, ct);
        merchant.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var create = await SendAsync(HttpMethod.Post, "/merchant/products", merchantToken, new
        {
            name = $"Timer product {Guid.NewGuid():N}",
            description = "A product for the checkout timer test.",
            categoryId = category.Id,
            variants = new[] { new { price = 120_000, stock = 1, weightGrams = 100 } },
        }, ct);
        create.StatusCode.ShouldBe(HttpStatusCode.Created);
        var product = (await create.Content.ReadFromJsonAsync<Product>(ct)).ShouldNotBeNull();
        using var published = await SendAsync(HttpMethod.Post, $"/merchant/products/{product.Id}/publish", merchantToken, null, ct);
        published.StatusCode.ShouldBe(HttpStatusCode.OK);

        var cart = await EventuallyAsync(
            () => SendAsync(HttpMethod.Post, "/cart/lines", buyerToken, new { variantId = product.Variants[0].Id, quantity = 1 }, ct),
            response => response.StatusCode == HttpStatusCode.OK);
        cart.Dispose();
        using var place = await SendAsync(HttpMethod.Post, "/checkout", buyerToken, new
        {
            deliveryAddress = new
            {
                recipientName = "Buyer",
                phone = "+84901234567",
                street = "12 Le Loi",
                ward = "Ben Nghe",
                province = "Hồ Chí Minh",
            },
            itemsTotal = 120_000,
        }, ct);
        place.StatusCode.ShouldBe(HttpStatusCode.Created);
        var checkout = (await place.Content.ReadFromJsonAsync<Checkout>(ct)).ShouldNotBeNull();

        var status = await EventuallyAsync(async () =>
        {
            using var response = await SendAsync(HttpMethod.Get, $"/checkout/{checkout.Id}", buyerToken, null, ct);
            return (response.StatusCode, Body: await response.Content.ReadFromJsonAsync<Checkout>(ct));
        }, result => result.StatusCode == HttpStatusCode.OK && result.Body?.Status == "Expired");
        status.Body!.ExpiresAt.ShouldBe(checkout.ExpiresAt);
    }

    private async Task<string> GetTokenAsync(string username, string password, CancellationToken ct)
    {
        using var response = await fixture.KeycloakClient.PostAsync(
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

    private Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string token, object? body, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = body is null ? null : JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return fixture.ApiClient.SendAsync(request, ct);
    }

    private static async Task<T> EventuallyAsync<T>(Func<Task<T>> read, Func<T, bool> until)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (true)
        {
            var value = await read();
            if (until(value)) return value;
            await Task.Delay(100, timeout.Token);
        }
    }
}
