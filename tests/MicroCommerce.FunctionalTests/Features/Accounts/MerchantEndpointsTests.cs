using System.Net;
using System.Net.Http.Json;
using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.FunctionalTests.Features.Accounts;

/// <summary>An Account opens and edits its one Merchant (User Story 1, FR-001 to FR-006).</summary>
public class MerchantEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Opening_a_merchant_returns_201_and_me_then_shows_it()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var name = CatalogTestData.UniqueShopName();

        var response = await client.PostAsJsonAsync("/api/me/merchant", MerchantHelpers.OpenRequest($"  {name}  "), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var shop = (await response.Content.ReadFromJsonAsync<ShopProfileDto>(Ct))!;
        shop.ShopName.ShouldBe(name);
        shop.Description.ShouldBe("Hand-thrown mugs");
        shop.PickupAddress.ShouldBe(MerchantHelpers.Address);
        shop.Version.ShouldBe(1);
        response.Headers.Location!.ToString().ShouldBe($"/api/marketplace/shops/{shop.MerchantId}");

        var (_, me) = await client.GETAsync<GetMeEndpoint, MeResponse>();
        me.Merchant.ShouldNotBeNull();
        me.Merchant.MerchantId.ShouldBe(shop.MerchantId);
        me.Merchant.ShopName.ShouldBe(name);
    }

    [Fact]
    public async Task Opening_without_a_token_is_unauthorized()
    {
        var response = await App.Client.PostAsJsonAsync("/api/me/merchant", MerchantHelpers.OpenRequest(CatalogTestData.UniqueShopName()), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_second_merchant_for_the_same_account_is_refused()
    {
        var (client, _, _) = await App.NewMerchantAsync();

        var response = await client.PostAsJsonAsync("/api/me/merchant", MerchantHelpers.OpenRequest(CatalogTestData.UniqueShopName()), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await response.ProblemAsync()).Type.ShouldBe("merchant-exists");
    }

    [Fact]
    public async Task A_shop_name_taken_in_another_letter_case_is_refused()
    {
        var (_, shop, _) = await App.NewMerchantAsync();

        var response = await App.ClientFor(TestActor.Buyer())
            .PostAsJsonAsync("/api/me/merchant", MerchantHelpers.OpenRequest(shop.ShopName.ToUpperInvariant()), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await response.ProblemAsync()).Type.ShouldBe("shop-name-taken");
    }

    [Fact]
    public async Task Invalid_input_names_each_invalid_field()
    {
        var response = await App.ClientFor(TestActor.Buyer()).PostAsJsonAsync(
            "/api/me/merchant",
            MerchantHelpers.OpenRequest("ab", new string('d', 2001), new AddressDto("", "District 1", "")),
            Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.ProblemAsync()).Errors!.Keys.Order()
            .ShouldBe(["description", "pickupAddress.province", "pickupAddress.street", "shopName"]);
    }

    [Fact]
    public async Task Parallel_opens_by_one_account_create_exactly_one_merchant()
    {
        var client = App.ClientFor(TestActor.Buyer());

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ =>
            client.PostAsJsonAsync("/api/me/merchant", MerchantHelpers.OpenRequest(CatalogTestData.UniqueShopName()), Ct)));

        responses.Count(r => r.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(9);
    }

    [Fact]
    public async Task Parallel_opens_with_one_name_create_exactly_one_merchant()
    {
        var name = CatalogTestData.UniqueShopName();

        var responses = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ =>
            App.ClientFor(TestActor.Buyer()).PostAsJsonAsync("/api/me/merchant", MerchantHelpers.OpenRequest(name), Ct)));

        responses.Count(r => r.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(9);
    }

    [Fact]
    public async Task The_owner_reads_its_shop_profile()
    {
        var (client, shop, _) = await App.NewMerchantAsync();

        var profile = await client.GetFromJsonAsync<ShopProfileDto>("/api/merchant/shop", Ct);

        profile.ShouldBe(shop);
    }

    [Fact]
    public async Task Merchant_centre_needs_a_merchant_and_a_sign_in()
    {
        var withoutMerchant = await App.ClientFor(TestActor.Buyer()).GetAsync("/api/merchant/shop", Ct);
        withoutMerchant.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await withoutMerchant.ProblemAsync()).Type.ShouldBe("merchant-required");

        (await App.Client.GetAsync("/api/merchant/shop", Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task The_owner_edits_its_shop_and_the_version_moves_on()
    {
        var (client, shop, _) = await App.NewMerchantAsync();
        var newName = CatalogTestData.UniqueShopName();
        var newAddress = new AddressDto("1 Tran Hung Dao", "District 5", "Ho Chi Minh City");

        var response = await client.PutAsJsonAsync("/api/merchant/shop",
            new { shopName = newName, description = "Cups too", pickupAddress = newAddress, version = shop.Version }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updated = (await response.Content.ReadFromJsonAsync<ShopProfileDto>(Ct))!;
        updated.ShouldBe(new ShopProfileDto(shop.MerchantId, newName, "Cups too", newAddress, shop.Version + 1));
    }

    [Fact]
    public async Task A_case_only_rename_of_its_own_name_is_allowed()
    {
        var (client, shop, _) = await App.NewMerchantAsync();

        var response = await client.PutAsJsonAsync("/api/merchant/shop",
            new { shopName = shop.ShopName.ToUpperInvariant(), description = (string?)null, pickupAddress = shop.PickupAddress, version = shop.Version }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Renaming_to_another_merchants_name_is_refused()
    {
        var (_, other, _) = await App.NewMerchantAsync();
        var (client, shop, _) = await App.NewMerchantAsync();

        var response = await client.PutAsJsonAsync("/api/merchant/shop",
            new { shopName = other.ShopName.ToLowerInvariant(), description = (string?)null, pickupAddress = shop.PickupAddress, version = shop.Version }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await response.ProblemAsync()).Type.ShouldBe("shop-name-taken");
    }

    [Fact]
    public async Task A_stale_version_is_refused()
    {
        var (client, shop, _) = await App.NewMerchantAsync();
        var edit = new { shopName = shop.ShopName, description = "First", pickupAddress = shop.PickupAddress, version = shop.Version };
        (await client.PutAsJsonAsync("/api/merchant/shop", edit, Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);

        var stale = await client.PutAsJsonAsync("/api/merchant/shop", edit with { description = "Second" }, Ct);

        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await stale.ProblemAsync()).Type.ShouldBe("shop-version-conflict");
    }

    [Fact]
    public async Task Editing_validates_and_needs_a_merchant()
    {
        var (client, shop, _) = await App.NewMerchantAsync();
        var invalid = await client.PutAsJsonAsync("/api/merchant/shop",
            new { shopName = "ab", description = (string?)null, pickupAddress = shop.PickupAddress, version = shop.Version }, Ct);
        invalid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await invalid.ProblemAsync()).Errors!.Keys.ShouldBe(["shopName"]);

        var edit = new { shopName = "Whatever Shop", description = (string?)null, pickupAddress = shop.PickupAddress, version = 1 };
        (await App.ClientFor(TestActor.Buyer()).PutAsJsonAsync("/api/merchant/shop", edit, Ct)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await App.Client.PutAsJsonAsync("/api/merchant/shop", edit, Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
