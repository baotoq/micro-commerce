using System.Net;
using System.Net.Http.Json;

namespace MicroCommerce.FunctionalTests;

public sealed record AddressDto(string Street, string District, string Province);

public sealed record ShopProfileDto(Guid MerchantId, string ShopName, string? Description, AddressDto PickupAddress, int Version);

public sealed record ProblemDto(int? Status, string? Type, string? Title, Dictionary<string, string[]>? Errors);

public static class MerchantHelpers
{
    public static readonly AddressDto Address = new("12 Le Loi", "District 1", "Ho Chi Minh City");

    public static object OpenRequest(string shopName, string? description = "Hand-thrown mugs", AddressDto? address = null) =>
        new { shopName, description, pickupAddress = address ?? Address };

    /// <summary>
    /// Opens a Merchant for <paramref name="actor"/> (a fresh Buyer by default) with a unique Shop name,
    /// and returns a client signed in as its owner.
    /// </summary>
    public static async Task<(HttpClient Client, ShopProfileDto Shop, TestActor Actor)> NewMerchantAsync(
        this ApiFixture app, TestActor? actor = null)
    {
        actor ??= TestActor.Buyer();
        var client = app.ClientFor(actor);
        var response = await client.PostAsJsonAsync("/api/me/merchant", OpenRequest(CatalogTestData.UniqueShopName()), TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var shop = (await response.Content.ReadFromJsonAsync<ShopProfileDto>(TestContext.Current.CancellationToken))!;
        return (client, shop, actor);
    }

    public static async Task<ProblemDto> ProblemAsync(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ProblemDto>(TestContext.Current.CancellationToken))!;
}
