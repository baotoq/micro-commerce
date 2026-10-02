using System.Net;
using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.FunctionalTests.Features.Accounts;

public class MerchantEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");

    // Shop names are unique across the shared database, so every test makes its own.
    private static string UniqueShopName(string prefix = "Shop") => $"{prefix} {Guid.NewGuid():N}";

    private Task<TestResult<MerchantResponse>> OpenAsync(HttpClient client, string shopName, PickupAddress? address = null) =>
        client.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(new(shopName, address ?? Address));

    [Fact]
    public async Task Opening_a_merchant_returns_created_and_shows_it_on_me()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var shopName = UniqueShopName();

        var (rsp, merchant) = await OpenAsync(client, shopName);

        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        rsp.Headers.Location!.ToString().ShouldBe("/merchant/shop");
        merchant.Id.Value.ShouldNotBe(Guid.Empty);
        merchant.ShopName.Value.ShouldBe(shopName);
        merchant.Description.ShouldBeNull();
        merchant.PickupAddress.ShouldBe(Address);

        var (_, me) = await client.GETAsync<GetMeEndpoint, MeResponse>();
        me.Merchant.ShouldBe(merchant);
    }

    [Fact]
    public async Task Me_without_a_merchant_has_none()
    {
        var (rsp, me) = await App.ClientFor(TestActor.Buyer()).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        me.Merchant.ShouldBeNull();
    }

    [Fact]
    public async Task Shop_name_is_trimmed()
    {
        var shopName = UniqueShopName();

        var (_, merchant) = await OpenAsync(App.ClientFor(TestActor.Buyer()), $"  {shopName}  ");

        merchant.ShopName.Value.ShouldBe(shopName);
    }

    [Fact]
    public async Task Opening_without_a_token_is_unauthorized()
    {
        var (rsp, _) = await OpenAsync(App.Client, UniqueShopName());

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Shop_name_taken_ignoring_case_is_a_conflict()
    {
        var shopName = UniqueShopName("Mug Corner");
        await OpenAsync(App.ClientFor(TestActor.Buyer()), shopName);

        var (rsp, problem) = await App.ClientFor(TestActor.Buyer())
            .POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, ProblemDetails>(new(shopName.ToUpperInvariant(), Address));

        rsp.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.Errors.Select(e => e.Name).ShouldBe(["shopName"]);
    }

    [Fact]
    public async Task Second_merchant_for_the_same_account_is_a_conflict()
    {
        var client = App.ClientFor(TestActor.Buyer());
        await OpenAsync(client, UniqueShopName());

        var (rsp, problem) = await client
            .POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, ProblemDetails>(new(UniqueShopName(), Address));

        rsp.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.Errors.ShouldHaveSingleItem().Name.ShouldBe("generalErrors");
    }

    [Fact]
    public async Task Concurrent_opens_for_the_same_account_create_one_merchant()
    {
        var buyer = TestActor.Buyer();
        // The Account exists first, so the race is on the Merchant, not on creating the Account.
        await App.ClientFor(buyer).GETAsync<GetMeEndpoint, MeResponse>();

        var results = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => App.ClientFor(buyer).POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
                new(UniqueShopName(), Address))));

        results.Count(r => r.Response.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        results.Count(r => r.Response.StatusCode == HttpStatusCode.Conflict).ShouldBe(4);
    }

    [Fact]
    public async Task Concurrent_opens_with_the_same_shop_name_create_one_merchant()
    {
        var shopName = UniqueShopName();
        var clients = Enumerable.Range(0, 5).Select(_ => App.ClientFor(TestActor.Buyer())).ToList();
        // The Accounts exist first, so the race is on the Shop name, not on creating the Accounts.
        await Task.WhenAll(clients.Select(c => c.GETAsync<GetMeEndpoint, MeResponse>()));

        var results = await Task.WhenAll(clients.Select(c =>
            c.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, ProblemDetails>(new(shopName, Address))));

        results.Count(r => r.Response.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        var conflicts = results.Where(r => r.Response.StatusCode == HttpStatusCode.Conflict).ToList();
        conflicts.Count.ShouldBe(4);
        conflicts.ShouldAllBe(r => r.Result.Errors.Single().Name == "shopName");
    }

    [Fact]
    public async Task Missing_shop_name_and_pickup_address_are_bad_requests()
    {
        var (rsp, problem) = await App.ClientFor(TestActor.Buyer())
            .POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, ProblemDetails>(new("  ", null!));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["shopName", "pickupAddress"], ignoreOrder: true);
    }

    [Fact]
    public async Task Opening_with_a_too_long_shop_name_is_a_bad_request()
    {
        var (rsp, problem) = await App.ClientFor(TestActor.Buyer())
            .POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, ProblemDetails>(new(new string('a', 101), Address));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["shopName"]);
    }

    [Fact]
    public async Task Incomplete_pickup_address_is_a_bad_request()
    {
        var (rsp, problem) = await App.ClientFor(TestActor.Buyer())
            .POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, ProblemDetails>(
                new(UniqueShopName(), new PickupAddress("12 Le Loi", "", "District 1", null!)));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name)
            .ShouldBe(["pickupAddress.ward", "pickupAddress.province"], ignoreOrder: true);
    }

    [Fact]
    public async Task Merchant_reads_its_own_shop()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var (_, opened) = await OpenAsync(client, UniqueShopName());

        var (rsp, shop) = await client.GETAsync<GetShopEndpoint, MerchantResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        shop.ShouldBe(opened);
    }

    [Fact]
    public async Task Each_account_acts_for_its_own_merchant()
    {
        var first = App.ClientFor(TestActor.Buyer());
        var second = App.ClientFor(TestActor.Buyer());
        var (_, firstMerchant) = await OpenAsync(first, UniqueShopName());
        var (_, secondMerchant) = await OpenAsync(second, UniqueShopName());

        var (_, firstShop) = await first.GETAsync<GetShopEndpoint, MerchantResponse>();
        var (_, secondShop) = await second.GETAsync<GetShopEndpoint, MerchantResponse>();

        firstShop.ShouldBe(firstMerchant);
        secondShop.ShouldBe(secondMerchant);
    }

    [Fact]
    public async Task Merchant_centre_refuses_an_account_without_a_merchant()
    {
        var (rsp, problem) = await App.ClientFor(TestActor.Buyer()).GETAsync<GetShopEndpoint, ProblemDetails>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        problem.Status.ShouldBe(403);
    }

    [Fact]
    public async Task Merchant_centre_refuses_anonymous_requests()
    {
        var (rsp, _) = await App.Client.GETAsync<GetShopEndpoint, MerchantResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Merchant_edits_its_shop()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var (_, opened) = await OpenAsync(client, UniqueShopName());
        var newName = UniqueShopName("Renamed");
        var newAddress = new PickupAddress("1 Tran Phu", "Hai Chau 1", "Hai Chau", "Da Nang");

        var (rsp, updated) = await client.PUTAsync<UpdateShopEndpoint, UpdateShopRequest, MerchantResponse>(
            new(newName, "Mugs and teapots.", newAddress));

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldBe(opened with { ShopName = ShopName.From(newName), Description = "Mugs and teapots.", PickupAddress = newAddress });

        var (_, shop) = await client.GETAsync<GetShopEndpoint, MerchantResponse>();
        shop.ShouldBe(updated);
    }

    [Fact]
    public async Task Merchant_can_change_the_case_of_its_own_shop_name()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var shopName = UniqueShopName("Tea House");
        await OpenAsync(client, shopName);

        var (rsp, updated) = await client.PUTAsync<UpdateShopEndpoint, UpdateShopRequest, MerchantResponse>(
            new(shopName.ToUpperInvariant(), null, Address));

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShopName.Value.ShouldBe(shopName.ToUpperInvariant());
    }

    [Fact]
    public async Task Renaming_to_a_taken_shop_name_is_a_conflict()
    {
        var taken = UniqueShopName("Taken");
        await OpenAsync(App.ClientFor(TestActor.Buyer()), taken);
        var client = App.ClientFor(TestActor.Buyer());
        await OpenAsync(client, UniqueShopName());

        var (rsp, problem) = await client.PUTAsync<UpdateShopEndpoint, UpdateShopRequest, ProblemDetails>(
            new(taken.ToLowerInvariant(), null, Address));

        rsp.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.Errors.Select(e => e.Name).ShouldBe(["shopName"]);
    }

    [Fact]
    public async Task Editing_with_an_incomplete_pickup_address_is_a_bad_request()
    {
        var client = App.ClientFor(TestActor.Buyer());
        await OpenAsync(client, UniqueShopName());

        var (rsp, problem) = await client.PUTAsync<UpdateShopEndpoint, UpdateShopRequest, ProblemDetails>(
            new("", null, Address with { Street = "" }));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["shopName", "pickupAddress.street"], ignoreOrder: true);
    }

    [Fact]
    public async Task Renaming_to_a_too_long_shop_name_is_a_bad_request()
    {
        var client = App.ClientFor(TestActor.Buyer());
        await OpenAsync(client, UniqueShopName());

        var (rsp, problem) = await client.PUTAsync<UpdateShopEndpoint, UpdateShopRequest, ProblemDetails>(
            new(new string('a', 101), null, Address));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["shopName"]);
    }

    [Fact]
    public async Task Editing_without_a_merchant_is_refused()
    {
        var (rsp, _) = await App.ClientFor(TestActor.Buyer())
            .PUTAsync<UpdateShopEndpoint, UpdateShopRequest, ProblemDetails>(new(UniqueShopName(), null, Address));

        rsp.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
