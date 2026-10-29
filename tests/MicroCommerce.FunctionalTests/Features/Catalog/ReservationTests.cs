using System.Diagnostics;
using System.Net;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

/// <summary>The Catalog's Reservation contract (ADR-0006), called through DI: it has no HTTP endpoint.</summary>
public class ReservationTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(15);

    private readonly HttpClient _operator = App.ClientFor(TestActor.PlatformOperator());

    // The database is shared across the test run, so every test makes its own Merchants and Products.
    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private static DateTimeOffset InAnHour => DateTimeOffset.UtcNow.AddHours(1);

    private static ReservationLine Line(VariantId variantId, int quantity) => new(variantId, quantity);

    private sealed record TestProduct(HttpClient Merchant, MerchantId MerchantId, ProductResponse Product)
    {
        public VariantId Red => Product.Variants.Single(v => v.OptionValues[0] == "Red").Id;
        public VariantId Blue => Product.Variants.Single(v => v.OptionValues[0] == "Blue").Id;
    }

    /// <summary>A Published Red/Blue mug of a fresh Merchant whose Shop has reached the Catalog: 5 Red, 2 Blue.</summary>
    private async Task<TestProduct> NewMugAsync(int redStock = 5, int blueStock = 2)
    {
        var merchant = App.ClientFor(TestActor.Buyer());
        var (openRsp, opened) = await merchant.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
            new(Unique("Shop"), Address));
        openRsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        await EventuallyAsync(
            async () => (await App.Client.GETAsync<GetShopPageEndpoint, GetShopPageRequest, ShopResponse>(new(opened.Id))).Response.StatusCode,
            status => status == HttpStatusCode.OK);

        var (_, category) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(
            new(Unique("Category"), null));
        var (createRsp, product) = await merchant.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(
            new(Unique("Mug"), null, category.Id, [new("Colour", ["Red", "Blue"])],
            [
                new(["Red"], 120_000, redStock, 350),
                new(["Blue"], 130_000, blueStock, 360),
            ]));
        createRsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await merchant.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return new(merchant, opened.Id, product);
    }

    /// <summary>Runs <paramref name="call"/> against the contract in its own scope, as a request would.</summary>
    private async Task<T> WithReservationsAsync<T>(Func<ICatalogReservations, Task<T>> call)
    {
        await using var scope = App.Services.CreateAsyncScope();
        return await call(scope.ServiceProvider.GetRequiredService<ICatalogReservations>());
    }

    private Task<ReserveResult> ReserveAsync(CheckoutId checkoutId, DateTimeOffset holdUntil, params ReservationLine[] lines) =>
        WithReservationsAsync(r => r.ReserveAsync(checkoutId, lines, holdUntil, TestContext.Current.CancellationToken));

    private Task<bool> CommitAsync(CheckoutId checkoutId) =>
        WithReservationsAsync(r => r.CommitAsync(checkoutId, TestContext.Current.CancellationToken));

    private Task ReleaseAsync(CheckoutId checkoutId) =>
        WithReservationsAsync(async r =>
        {
            await r.ReleaseAsync(checkoutId, TestContext.Current.CancellationToken);
            return true;
        });

    /// <summary>What the Lookup says can be bought of <paramref name="variantId"/>.</summary>
    private async Task<int> AvailableAsync(VariantId variantId)
    {
        await using var scope = App.Services.CreateAsyncScope();
        var variants = await scope.ServiceProvider.GetRequiredService<ICatalogLookup>()
            .LookupAsync([variantId], TestContext.Current.CancellationToken);
        return variants[variantId].Available;
    }

    private async Task<(string Cache, StorefrontProductResponse Product)> StorefrontAsync(ProductId id)
    {
        var (rsp, product) = await App.Client.GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(new(id));
        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (rsp.Headers.GetValues("X-Cache").Single(), product);
    }

    private static int StockOf(StorefrontProductResponse product, VariantId id) => product.Variants.Single(v => v.Id == id).Stock;

    private static async Task<T> EventuallyAsync<T>(Func<Task<T>> read, Func<T, bool> until)
    {
        var stopwatch = Stopwatch.StartNew();
        while (true)
        {
            var value = await read();
            if (until(value)) return value;
            if (stopwatch.Elapsed > EventTimeout)
                throw new ShouldAssertException($"Still not as expected after {EventTimeout}: {value}");
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Reserve_lowers_what_is_available_but_not_the_stock()
    {
        var mug = await NewMugAsync();

        var result = await ReserveAsync(CheckoutId.New(), InAnHour, Line(mug.Red, 2));

        result.Succeeded.ShouldBeTrue();
        (await AvailableAsync(mug.Red)).ShouldBe(3);
        (await AvailableAsync(mug.Blue)).ShouldBe(2);
        var (_, merchantView) = await mug.Merchant.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(new(mug.Product.Id));
        merchantView.Variants.Single(v => v.Id == mug.Red).Stock.ShouldBe(5, "the Merchant sees the on-hand stock");
    }

    [Fact]
    public async Task Reserve_with_one_failing_line_reserves_nothing_and_says_why()
    {
        var mug = await NewMugAsync();
        var gone = VariantId.New();

        var result = await ReserveAsync(CheckoutId.New(), InAnHour, Line(mug.Red, 1), Line(mug.Blue, 3), Line(gone, 1));

        result.Succeeded.ShouldBeFalse();
        result.Failures.ShouldBe(
        [
            new(mug.Blue, ReservationFailureReason.NotEnoughAvailable, 2),
            new(gone, ReservationFailureReason.NotBuyable, 0),
        ], ignoreOrder: true);
        (await AvailableAsync(mug.Red)).ShouldBe(5);
    }

    [Fact]
    public async Task Variant_of_an_unpublished_product_is_not_buyable()
    {
        var mug = await NewMugAsync();
        await mug.Merchant.POSTAsync<UnpublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(mug.Product.Id));

        var result = await ReserveAsync(CheckoutId.New(), InAnHour, Line(mug.Red, 1));

        result.Failures.ShouldBe([new(mug.Red, ReservationFailureReason.NotBuyable, 0)]);
    }

    [Fact]
    public async Task Lines_for_the_same_variant_are_reserved_together()
    {
        var mug = await NewMugAsync();

        (await ReserveAsync(CheckoutId.New(), InAnHour, Line(mug.Red, 2), Line(mug.Red, 2))).Succeeded.ShouldBeTrue();
        (await AvailableAsync(mug.Red)).ShouldBe(1);
        (await ReserveAsync(CheckoutId.New(), InAnHour, Line(mug.Red, 1), Line(mug.Red, 1))).Failures
            .ShouldBe([new(mug.Red, ReservationFailureReason.NotEnoughAvailable, 1)]);
    }

    [Fact]
    public async Task Repeating_reserve_commit_or_release_changes_nothing_further()
    {
        var mug = await NewMugAsync();
        var checkoutId = CheckoutId.New();

        (await ReserveAsync(checkoutId, InAnHour, Line(mug.Red, 2))).Succeeded.ShouldBeTrue();
        (await ReserveAsync(checkoutId, InAnHour, Line(mug.Red, 2), Line(mug.Blue, 1))).Succeeded.ShouldBeTrue();
        (await AvailableAsync(mug.Red)).ShouldBe(3);
        (await AvailableAsync(mug.Blue)).ShouldBe(2);

        (await CommitAsync(checkoutId)).ShouldBeTrue();
        (await CommitAsync(checkoutId)).ShouldBeTrue();
        (await AvailableAsync(mug.Red)).ShouldBe(3);

        await ReleaseAsync(checkoutId);
        await ReleaseAsync(checkoutId);
        (await AvailableAsync(mug.Red)).ShouldBe(5);
        (await CommitAsync(checkoutId)).ShouldBeFalse("a Released Checkout can't be committed");
        (await AvailableAsync(mug.Red)).ShouldBe(5);
    }

    [Fact]
    public async Task Commit_or_release_of_an_unknown_checkout_does_nothing()
    {
        (await CommitAsync(CheckoutId.New())).ShouldBeFalse();
        await ReleaseAsync(CheckoutId.New());
    }

    [Fact]
    public async Task Release_frees_reserved_and_held_stock()
    {
        var mug = await NewMugAsync();
        var reserved = CheckoutId.New();
        var held = CheckoutId.New();
        await ReserveAsync(reserved, InAnHour, Line(mug.Red, 1));
        await ReserveAsync(held, InAnHour, Line(mug.Red, 2));
        await CommitAsync(held);

        await ReleaseAsync(reserved);
        (await AvailableAsync(mug.Red)).ShouldBe(3);
        await ReleaseAsync(held);
        (await AvailableAsync(mug.Red)).ShouldBe(5);
    }

    [Fact]
    public async Task Reserved_stock_comes_back_by_itself_after_its_deadline_but_held_stock_does_not()
    {
        var mug = await NewMugAsync();
        var lapsing = CheckoutId.New();
        var committed = CheckoutId.New();
        var soon = DateTimeOffset.UtcNow.AddSeconds(2);
        await ReserveAsync(lapsing, soon, Line(mug.Red, 1));
        await ReserveAsync(committed, soon, Line(mug.Blue, 1));
        await CommitAsync(committed);

        (await AvailableAsync(mug.Red)).ShouldBe(4);
        await EventuallyAsync(() => AvailableAsync(mug.Red), available => available == 5);
        (await AvailableAsync(mug.Blue)).ShouldBe(1);
    }

    [Fact]
    public async Task Only_one_of_two_simultaneous_reserves_gets_the_last_unit()
    {
        var mug = await NewMugAsync(blueStock: 1);
        var ct = TestContext.Current.CancellationToken;

        // Hold the Variant's row, so both Reserves are waiting for it at once; when it is let go, one takes the unit
        // and the other then finds none left.
        var connectionString = App.Services.GetRequiredService<IConfiguration>().GetConnectionString("micro-commerce-db");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using (var lockVariant = new NpgsqlCommand(
            """SELECT 1 FROM catalog."Variants" WHERE "Id" = @id FOR UPDATE""", connection, transaction))
        {
            lockVariant.Parameters.AddWithValue("id", mug.Blue.Value);
            (await lockVariant.ExecuteScalarAsync(ct)).ShouldNotBeNull();
        }

        var reserves = new[] { CheckoutId.New(), CheckoutId.New() }
            .Select(id => ReserveAsync(id, InAnHour, Line(mug.Blue, 1)))
            .ToList();
        // Polled outside the locking transaction, which would keep reading the activity snapshot it first took.
        await EventuallyAsync(async () =>
        {
            await using var probe = new NpgsqlConnection(connectionString);
            await probe.OpenAsync(ct);
            await using var waiting = new NpgsqlCommand(
                """SELECT count(*) FROM pg_stat_activity WHERE wait_event_type = 'Lock' AND query LIKE '%"Variants"%FOR UPDATE%'""", probe);
            return (long)(await waiting.ExecuteScalarAsync(ct))!;
        }, count => count >= 2);
        await transaction.RollbackAsync(ct);

        var results = await Task.WhenAll(reserves);
        results.Count(r => r.Succeeded).ShouldBe(1);
        results.Single(r => !r.Succeeded).Failures.ShouldBe([new(mug.Blue, ReservationFailureReason.NotEnoughAvailable, 0)]);
        (await AvailableAsync(mug.Blue)).ShouldBe(0);
    }

    [Fact]
    public async Task Reservation_outlives_its_variant_being_edited_away()
    {
        var mug = await NewMugAsync();
        var checkoutId = CheckoutId.New();
        await ReserveAsync(checkoutId, InAnHour, Line(mug.Blue, 1));

        var (editRsp, _) = await mug.Merchant.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProductResponse>(
            new(mug.Product.Id, mug.Product.Name, null, mug.Product.CategoryId, [new("Colour", ["Red"])], [new(["Red"], 120_000, 5, 350)]));
        editRsp.StatusCode.ShouldBe(HttpStatusCode.OK);

        (await CommitAsync(checkoutId)).ShouldBeTrue();
        await ReleaseAsync(checkoutId);
        (await CommitAsync(checkoutId)).ShouldBeFalse();
    }

    [Fact]
    public async Task Lowering_stock_below_what_is_reserved_keeps_the_reservation_and_blocks_new_ones()
    {
        var mug = await NewMugAsync();
        var checkoutId = CheckoutId.New();
        await ReserveAsync(checkoutId, InAnHour, Line(mug.Red, 3));

        var (editRsp, _) = await mug.Merchant.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProductResponse>(
            new(mug.Product.Id, mug.Product.Name, null, mug.Product.CategoryId, [new("Colour", ["Red", "Blue"])],
                [new(["Red"], 120_000, 1, 350), new(["Blue"], 130_000, 2, 360)]));
        editRsp.StatusCode.ShouldBe(HttpStatusCode.OK);

        (await AvailableAsync(mug.Red)).ShouldBe(0);
        (await CommitAsync(checkoutId)).ShouldBeTrue();
        (await ReserveAsync(CheckoutId.New(), InAnHour, Line(mug.Red, 1))).Failures
            .ShouldBe([new(mug.Red, ReservationFailureReason.NotEnoughAvailable, 0)]);
    }

    [Fact]
    public async Task Storefront_shows_available_stock_and_reserve_commit_and_release_evict_the_cached_detail()
    {
        var mug = await NewMugAsync();
        var checkoutId = CheckoutId.New();

        (await StorefrontAsync(mug.Product.Id)).Cache.ShouldBe("MISS");
        (await StorefrontAsync(mug.Product.Id)).Cache.ShouldBe("HIT");

        await ReserveAsync(checkoutId, InAnHour, Line(mug.Red, 2));
        var (afterReserve, reserved) = await StorefrontAsync(mug.Product.Id);
        afterReserve.ShouldBe("MISS");
        StockOf(reserved, mug.Red).ShouldBe(3);
        StockOf(reserved, mug.Blue).ShouldBe(2);
        (await StorefrontAsync(mug.Product.Id)).Cache.ShouldBe("HIT");

        await CommitAsync(checkoutId);
        var (afterCommit, committed) = await StorefrontAsync(mug.Product.Id);
        afterCommit.ShouldBe("MISS");
        StockOf(committed, mug.Red).ShouldBe(3);

        await ReleaseAsync(checkoutId);
        var (afterRelease, released) = await StorefrontAsync(mug.Product.Id);
        afterRelease.ShouldBe("MISS");
        StockOf(released, mug.Red).ShouldBe(5);
    }

    [Fact]
    public async Task Storefront_list_is_out_of_stock_once_everything_is_reserved()
    {
        var mug = await NewMugAsync(redStock: 1, blueStock: 1);
        var checkoutId = CheckoutId.New();

        await ReserveAsync(checkoutId, InAnHour, Line(mug.Red, 1), Line(mug.Blue, 1));
        (await ListedAsync(mug.MerchantId)).InStock.ShouldBeFalse();

        await ReleaseAsync(checkoutId);
        (await ListedAsync(mug.MerchantId)).InStock.ShouldBeTrue();
    }

    private async Task<StorefrontProductSummaryResponse> ListedAsync(MerchantId shopId)
    {
        var (rsp, page) = await App.Client.GETAsync<ListStorefrontProductsEndpoint, ListStorefrontProductsRequest,
            PagedResponse<StorefrontProductSummaryResponse>>(new() { ShopId = shopId });
        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return page.Items.ShouldHaveSingleItem();
    }
}
