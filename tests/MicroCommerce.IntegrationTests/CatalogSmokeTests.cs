using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MicroCommerce.IntegrationTests;

/// <summary>
/// The whole Catalog through the real AppHost: a seeded Merchant owner signs in with Keycloak, opens a
/// Merchant, lists a Product in the starter Category tree, uploads an image to Azurite and publishes;
/// an anonymous visitor then finds the Product and downloads its image (User Stories 1, 2, 3 and 5).
/// </summary>
public class CatalogSmokeTests(AppHostFixture fixture)
{
    private sealed record Node(Guid Id, string Name, List<Node> Children);

    private sealed record Created(Guid Id, int Version, List<Image> Images);

    private sealed record Image(Guid Id, string Url);

    private sealed record Card(Guid Id, string? CoverImageUrl);

    private sealed record Page(List<Card> Items);

    [Fact]
    public async Task A_merchant_lists_a_product_with_an_image_and_a_visitor_finds_it()
    {
        var ct = TestContext.Current.CancellationToken;
        var api = fixture.ApiClient;
        var bearer = new AuthenticationHeaderValue("Bearer", await fixture.MerchantTokenAsync(ct));
        var name = $"Smoke mug {Guid.NewGuid():N}"[..20];

        await SendAsync(HttpMethod.Post, "/api/me/merchant", new
        {
            shopName = $"Smoke Shop {Guid.NewGuid():N}"[..30],
            description = "End-to-end",
            pickupAddress = new { street = "12 Le Loi", district = "District 1", province = "Ho Chi Minh City" },
        }, HttpStatusCode.Created);

        var tree = (await api.GetFromJsonAsync<List<Node>>("/api/marketplace/categories", ct))!;
        var kitchen = tree.Single(c => c.Name == "Home").Children.Single(c => c.Name == "Kitchen").Id;

        var product = (await (await SendAsync(HttpMethod.Post, "/api/merchant/products", new
        {
            name,
            description = "A mug for the smoke test",
            categoryId = kitchen,
            options = Array.Empty<object>(),
            variants = new[] { new { optionValues = Array.Empty<string>(), price = 120_000, stock = 5, weightGrams = 400 } },
        }, HttpStatusCode.Created)).Content.ReadFromJsonAsync<Created>(ct))!;

        using var upload = new HttpRequestMessage(HttpMethod.Post, $"/api/merchant/products/{product.Id}/images");
        upload.Headers.Authorization = bearer;
        var png = new ByteArrayContent([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0x0D, 1, 2, 3, 4]);
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        upload.Content = new MultipartFormDataContent { { png, "file", "mug.png" } };
        var uploaded = await api.SendAsync(upload, ct);
        uploaded.StatusCode.ShouldBe(HttpStatusCode.Created);
        var withImage = (await uploaded.Content.ReadFromJsonAsync<Created>(ct))!;

        await SendAsync(HttpMethod.Post, $"/api/merchant/products/{product.Id}/publish", new { version = withImage.Version }, HttpStatusCode.OK);

        // An anonymous visitor.
        var page = (await api.GetFromJsonAsync<Page>($"/api/marketplace/products?query={Uri.EscapeDataString(name)}", ct))!;
        var card = page.Items.ShouldHaveSingleItem();
        card.Id.ShouldBe(product.Id);
        var image = await api.GetAsync(card.CoverImageUrl!, ct);
        image.StatusCode.ShouldBe(HttpStatusCode.OK);
        image.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");

        async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object body, HttpStatusCode expected)
        {
            using var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body, options: JsonSerializerOptions.Web) };
            request.Headers.Authorization = bearer;
            var response = await api.SendAsync(request, ct);
            response.StatusCode.ShouldBe(expected, await response.Content.ReadAsStringAsync(ct));
            return response;
        }
    }
}
