using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Features.Ordering;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MicroCommerce.FunctionalTests.Features.Ordering;

public class CheckoutEndpointsTests(DevelopmentApiFixture App) : TestBase<DevelopmentApiFixture>
{
    private static readonly DeliveryAddress Address = new()
    {
        RecipientName = "Buyer",
        Phone = "+84901234567",
        Street = "12 Le Loi",
        Ward = "Ben Nghe",
        Province = "Hồ Chí Minh",
    };

    private static readonly ProductOption Colour = new("Colour", ["Red", "Blue"]);
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(15);
    private readonly HttpClient _operator = App.ClientFor(TestActor.PlatformOperator());

    private sealed record TestMerchant(HttpClient Client, MerchantResponse Merchant)
    {
        public MerchantId Id => Merchant.Id;
    }

    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private async Task<TestMerchant> NewMerchantAsync()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var (response, merchant) = await client.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
            new(Unique("Shop"), new("12 Le Loi", "Ben Nghe", "District 1", "Hồ Chí Minh")));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await EventuallyAsync(
            async () => (await App.Client.GETAsync<GetShopPageEndpoint, GetShopPageRequest, ShopResponse>(new(merchant.Id))).Response.StatusCode,
            status => status == HttpStatusCode.OK);
        return new(client, merchant);
    }

    private async Task<CategoryId> NewCategoryAsync()
    {
        var (_, category) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(
            new(Unique("Category"), null));
        return category.Id;
    }

    private async Task<ProductResponse> NewPublishedProductAsync(TestMerchant merchant, int stock = 5)
    {
        var (_, product) = await merchant.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(
            new(Unique("Mug"), null, await NewCategoryAsync(), [Colour],
            [
                new(["Red"], 120_000, stock, 350),
                new(["Blue"], 130_000, stock, 360),
            ]));
        var (publishResponse, published) = await merchant.Client
            .POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        publishResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        return published;
    }

    private static VariantId Red(ProductResponse product) => product.Variants.Single(v => v.OptionValues[0] == "Red").Id;
    private static VariantId Blue(ProductResponse product) => product.Variants.Single(v => v.OptionValues[0] == "Blue").Id;

    private static async Task<CartResponse> AddAsync(HttpClient buyer, VariantId id, int quantity = 1)
    {
        var (response, cart) = await buyer.POSTAsync<AddCartLineEndpoint, AddCartLineRequest, CartResponse>(new(id, quantity));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return cart;
    }

    private static async Task<(HttpResponseMessage Response, CheckoutResponse Checkout)> PlaceAsync(HttpClient buyer, long itemsTotal)
    {
        var (response, checkout) = await buyer.POSTAsync<PlaceCheckoutEndpoint, PlaceCheckoutRequest, CheckoutResponse>(
            new(Address, itemsTotal));
        return (response, checkout);
    }

    private static async Task<CheckoutConflictResponse> ConflictAsync(HttpClient buyer, long itemsTotal = 0)
    {
        using var response = await buyer.PostAsJsonAsync("/checkout", new PlaceCheckoutRequest(Address, itemsTotal));
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        return (await response.Content.ReadFromJsonAsync<CheckoutConflictResponse>(
            TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    private static async Task<CartResponse> GetCartAsync(HttpClient buyer)
    {
        var (response, cart) = await buyer.GETAsync<GetCartEndpoint, CartResponse>();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return cart;
    }

    private static async Task<T> EventuallyAsync<T>(Func<Task<T>> read, Func<T, bool> until)
    {
        var timer = Stopwatch.StartNew();
        while (true)
        {
            var value = await read();
            if (until(value)) return value;
            if (timer.Elapsed > EventTimeout) throw new ShouldAssertException($"Still not as expected after {EventTimeout}: {value}");
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Placement_freezes_lines_clears_the_cart_and_expiry_releases_stock()
    {
        var merchant = await NewMerchantAsync();
        var product = await NewPublishedProductAsync(merchant);
        var buyer = App.ClientFor(TestActor.Buyer());
        await AddAsync(buyer, Red(product), 2);

        var (placed, checkout) = await PlaceAsync(buyer, 240_000);

        placed.StatusCode.ShouldBe(HttpStatusCode.Created);
        checkout.Status.ShouldBe(CheckoutStatus.AwaitingPayment);
        checkout.ExpiresAt.ShouldBeGreaterThan(checkout.PlacedAt);
        checkout.ItemsTotal.ShouldBe(Money.From(240_000));
        checkout.ShippingTotal.ShouldBe(Money.From(30_000));
        checkout.Total.ShouldBe(Money.From(270_000));
        checkout.Merchants.ShouldHaveSingleItem().Subtotal.ShouldBe(Money.From(240_000));
        checkout.Lines.ShouldHaveSingleItem().ProductName.ShouldBe(product.Name);
        checkout.Lines.Single().OptionValues.ShouldBe(["Red"]);
        (await GetCartAsync(buyer)).Lines.ShouldBeEmpty();

        var (secondResponse, secondConflict) = await buyer
            .POSTAsync<PlaceCheckoutEndpoint, PlaceCheckoutRequest, CheckoutConflictResponse>(new(Address, 0));
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        secondConflict.ExistingCheckoutId.ShouldBe(checkout.Id);

        var (otherGet, _) = await App.ClientFor(TestActor.Buyer())
            .GETAsync<GetCheckoutEndpoint, GetCheckoutRequest, CheckoutResponse>(new(checkout.Id));
        otherGet.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var otherExpiry = await App.ClientFor(TestActor.Buyer()).PostAsJsonAsync(
            $"/dev/checkouts/{checkout.Id}/expire", new { }, TestContext.Current.CancellationToken);
        otherExpiry.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await buyer.GETAsync<ListCheckoutsEndpoint, ListCheckoutsRequest, CheckoutListResponse>(new()))
            .Result.Items.Select(c => c.Id).ShouldContain(checkout.Id);

        var detailBeforeEdit = await App.Client.GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(
            new(product.Id));
        detailBeforeEdit.Result.Variants.Single(v => v.Id == Red(product)).Stock.ShouldBe(3);

        await merchant.Client.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProductResponse>(
            new(product.Id, "Renamed mug", null, product.CategoryId, [Colour],
            [new(["Red"], 140_000, 5, 400), new(["Blue"], 150_000, 5, 410)]));
        var (get, frozen) = await buyer.GETAsync<GetCheckoutEndpoint, GetCheckoutRequest, CheckoutResponse>(new(checkout.Id));
        get.StatusCode.ShouldBe(HttpStatusCode.OK);
        frozen.Lines.Single().ProductName.ShouldBe(product.Name);
        frozen.Lines.Single().UnitPrice.ShouldBe(Money.From(120_000));
        frozen.Lines.Single().OptionValues.ShouldBe(["Red"]);

        var expiry = await buyer.PostAsJsonAsync(
            $"/dev/checkouts/{checkout.Id}/expire",
            new { },
            TestContext.Current.CancellationToken);
        expiry.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        await EventuallyAsync(async () =>
        {
            var (_, current) = await buyer.GETAsync<GetCheckoutEndpoint, GetCheckoutRequest, CheckoutResponse>(new(checkout.Id));
            return current.Status;
        }, status => status == CheckoutStatus.Expired);
        var available = await EventuallyAsync(async () =>
        {
            var (_, current) = await App.Client.GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(
                new(product.Id));
            return current.Variants.Single(v => v.Id == Red(product)).Stock;
        }, stock => stock == 5);
        available.ShouldBe(5);

        var secondExpiry = await buyer.PostAsJsonAsync(
            $"/dev/checkouts/{checkout.Id}/expire",
            new { },
            TestContext.Current.CancellationToken);
        secondExpiry.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        await Task.Delay(200, TestContext.Current.CancellationToken);
        var (_, expiredAgain) = await buyer.GETAsync<GetCheckoutEndpoint, GetCheckoutRequest, CheckoutResponse>(new(checkout.Id));
        expiredAgain.Status.ShouldBe(CheckoutStatus.Expired);
        using var payExpired = await buyer.PostAsJsonAsync(
            $"/dev/checkouts/{checkout.Id}/pay", new { }, TestContext.Current.CancellationToken);
        payExpired.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Payment_creates_one_scoped_order_per_merchant_from_the_frozen_checkout()
    {
        var firstMerchant = await NewMerchantAsync();
        var secondMerchant = await NewMerchantAsync();
        var firstProduct = await NewPublishedProductAsync(firstMerchant);
        var secondProduct = await NewPublishedProductAsync(secondMerchant);
        var buyer = App.ClientFor(TestActor.Buyer());
        await AddAsync(buyer, Red(firstProduct));
        await AddAsync(buyer, Blue(secondProduct));
        var (placed, checkout) = await PlaceAsync(buyer, 250_000);
        placed.StatusCode.ShouldBe(HttpStatusCode.Created);

        var (suspended, _) = await _operator.POSTAsync<SuspendMerchantEndpoint, ChangeMerchantStatusRequest, OperatorMerchantResponse>(
            new(firstMerchant.Id));
        suspended.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var pay = await buyer.PostAsJsonAsync(
            $"/dev/checkouts/{checkout.Id}/pay", new { }, TestContext.Current.CancellationToken);
        pay.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        var paid = await EventuallyAsync(async () =>
        {
            var (response, current) = await buyer.GETAsync<GetCheckoutEndpoint, GetCheckoutRequest, CheckoutResponse>(new(checkout.Id));
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            return current;
        }, current => current.Status == CheckoutStatus.Paid && current.Orders.Count == 2);
        paid.Orders.Select(order => order.MerchantId).ShouldBe([firstMerchant.Id, secondMerchant.Id], ignoreOrder: true);
        paid.Orders.Select(order => order.Status).ShouldAllBe(status => status == OrderStatus.ToShip);
        paid.Orders.Single(order => order.MerchantId == firstMerchant.Id).Subtotal.ShouldBe(Money.From(120_000));
        paid.Orders.Single(order => order.MerchantId == firstMerchant.Id).ShippingFee.ShouldBe(Money.From(30_000));
        paid.Orders.Single(order => order.MerchantId == firstMerchant.Id).Lines
            .Select(line => line.VariantId).ShouldHaveSingleItem().ShouldBe(Red(firstProduct));
        paid.Orders.Single(order => order.MerchantId == secondMerchant.Id).Subtotal.ShouldBe(Money.From(130_000));
        paid.Orders.Single(order => order.MerchantId == secondMerchant.Id).ShippingFee.ShouldBe(Money.From(30_000));
        paid.Orders.Single(order => order.MerchantId == secondMerchant.Id).Lines
            .Select(line => line.VariantId).ShouldHaveSingleItem().ShouldBe(Blue(secondProduct));
        paid.Orders.ShouldAllBe(order => order.DeliveryAddress == Address);
        var (checkoutListResponse, checkoutList) =
            await buyer.GETAsync<ListCheckoutsEndpoint, ListCheckoutsRequest, CheckoutListResponse>(new());
        checkoutListResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        checkoutList.Items.Single(item => item.Id == checkout.Id).Orders.Count.ShouldBe(2);

        using var duplicatePay = await buyer.PostAsJsonAsync(
            $"/dev/checkouts/{checkout.Id}/pay", new { }, TestContext.Current.CancellationToken);
        duplicatePay.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        var firstOrders = await firstMerchant.Client.GETAsync<ListMerchantOrdersEndpoint, ListMerchantOrdersRequest, OrderListResponse>(new());
        var secondOrders = await secondMerchant.Client.GETAsync<ListMerchantOrdersEndpoint, ListMerchantOrdersRequest, OrderListResponse>(new());
        firstOrders.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondOrders.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        firstOrders.Result.TotalCount.ShouldBe(1);
        secondOrders.Result.TotalCount.ShouldBe(1);
        var firstOrder = firstOrders.Result.Items.ShouldHaveSingleItem();
        firstOrder.MerchantId.ShouldBe(firstMerchant.Id);
        firstOrder.DeliveryAddress.ShouldBe(Address);
        firstOrder.Lines.Select(line => line.VariantId).ShouldHaveSingleItem().ShouldBe(Red(firstProduct));
        var secondOrder = secondOrders.Result.Items.ShouldHaveSingleItem();
        secondOrder.MerchantId.ShouldBe(secondMerchant.Id);
        secondOrder.DeliveryAddress.ShouldBe(Address);
        secondOrder.Lines.Select(line => line.VariantId).ShouldHaveSingleItem().ShouldBe(Blue(secondProduct));

        var otherBuyer = App.ClientFor(TestActor.Buyer());
        var (otherGet, _) = await otherBuyer.GETAsync<GetCheckoutEndpoint, GetCheckoutRequest, CheckoutResponse>(new(checkout.Id));
        otherGet.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var otherPay = await otherBuyer.PostAsJsonAsync(
            $"/dev/checkouts/{checkout.Id}/pay", new { }, TestContext.Current.CancellationToken);
        otherPay.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await using var scope = App.Services.CreateAsyncScope();
        var reservations = await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Reservations.AsNoTracking()
            .Where(reservation => reservation.CheckoutId == checkout.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        reservations.Count.ShouldBe(2);
        reservations.ShouldAllBe(reservation => reservation.State == ReservationState.Held && reservation.HoldUntil == null);
    }

    [Fact]
    public async Task Empty_cart_and_changed_items_total_conflict_without_reserving_stock()
    {
        var buyer = App.ClientFor(TestActor.Buyer());
        (await ConflictAsync(buyer)).Lines.ShouldBeEmpty();

        var product = await NewPublishedProductAsync(await NewMerchantAsync());
        await AddAsync(buyer, Red(product));
        var conflict = await ConflictAsync(buyer, 120_001);
        conflict.Detail.ShouldContain("items total changed");
        (await GetCartAsync(buyer)).Lines.ShouldHaveSingleItem();
        var (_, storefront) = await App.Client.GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(
            new(product.Id));
        storefront.Variants.Single(v => v.Id == Red(product)).Stock.ShouldBe(5);
    }

    [Fact]
    public async Task Unavailable_or_own_merchant_lines_are_returned_as_conflicts()
    {
        var merchant = await NewMerchantAsync();
        var product = await NewPublishedProductAsync(merchant);
        await AddAsync(merchant.Client, Red(product));

        var conflict = await ConflictAsync(merchant.Client, 120_000);

        conflict.Lines.ShouldHaveSingleItem().Reason.ShouldContain("own Shop");
        (await GetCartAsync(merchant.Client)).Lines.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Two_buyers_racing_for_the_last_unit_have_one_success()
    {
        var product = await NewPublishedProductAsync(await NewMerchantAsync(), stock: 1);
        var first = App.ClientFor(TestActor.Buyer());
        var second = App.ClientFor(TestActor.Buyer());
        await AddAsync(first, Red(product));
        await AddAsync(second, Red(product));

        var results = await Task.WhenAll(PlaceAsync(first, 120_000), PlaceAsync(second, 120_000));

        results.Select(r => r.Response.StatusCode).Count(status => status == HttpStatusCode.Created).ShouldBe(1);
        results.Select(r => r.Response.StatusCode).Count(status => status == HttpStatusCode.Conflict).ShouldBe(1);
        var carts = await Task.WhenAll(GetCartAsync(first), GetCartAsync(second));
        carts.Count(c => c.Lines.Count == 0).ShouldBe(1);
        carts.Count(c => c.Lines.Count == 1).ShouldBe(1);
    }

    [Fact]
    public async Task Two_simultaneous_placements_for_one_account_have_one_success()
    {
        var product = await NewPublishedProductAsync(await NewMerchantAsync());
        var actor = TestActor.Buyer();
        var buyer = App.ClientFor(actor);
        await AddAsync(buyer, Red(product));

        var connectionString = App.Services.GetRequiredService<IConfiguration>().GetConnectionString("micro-commerce-db");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(TestContext.Current.CancellationToken);
        await using (var lockCart = new NpgsqlCommand(
            """
            SELECT c."AccountId" FROM ordering."Carts" c
            JOIN accounts."Accounts" a ON a."Id" = c."AccountId"
            WHERE a."Subject" = @subject FOR UPDATE OF c
            """, connection, transaction))
        {
            lockCart.Parameters.AddWithValue("subject", actor.Subject);
            (await lockCart.ExecuteScalarAsync(TestContext.Current.CancellationToken)).ShouldNotBeNull();
        }

        var placements = Enumerable.Range(0, 2).Select(_ => PlaceAsync(buyer, 120_000)).ToList();
        await EventuallyAsync(
            () => LockWaits.CountBlockedByAsync(connectionString!, connection, TestContext.Current.CancellationToken),
            count => count >= 2);
        await transaction.RollbackAsync(TestContext.Current.CancellationToken);

        var results = await Task.WhenAll(placements);
        results.Select(r => r.Response.StatusCode).ShouldBe([HttpStatusCode.Created, HttpStatusCode.Conflict], ignoreOrder: true);
    }
}
