using System.Diagnostics;
using System.Net;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Ordering;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MicroCommerce.FunctionalTests.Features.Ordering;

public class CartEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");
    private static readonly ProductOption Colour = new("Colour", ["Red", "Blue"]);

    /// <summary>How long the Catalog may take to see a Merchant event; it normally takes milliseconds.</summary>
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(15);

    private readonly HttpClient _operator = App.ClientFor(TestActor.PlatformOperator());

    // The database is shared across the test run, so every test makes its own Accounts, Merchants and Products.
    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private sealed record TestMerchant(HttpClient Client, MerchantResponse Merchant)
    {
        public MerchantId Id => Merchant.Id;
        public string ShopName => Merchant.ShopName.Value;
    }

    /// <summary>A fresh Merchant, once its Shop has reached the Catalog, so its Products can be bought.</summary>
    private async Task<TestMerchant> NewMerchantAsync(HttpClient? client = null)
    {
        client ??= App.ClientFor(TestActor.Buyer());
        var (rsp, merchant) = await client.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
            new(Unique("Shop"), Address));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);

        await EventuallyAsync(
            async () => (await App.Client.GETAsync<GetShopPageEndpoint, GetShopPageRequest, ShopResponse>(new(merchant.Id))).Response.StatusCode,
            status => status == HttpStatusCode.OK);
        return new(client, merchant);
    }

    private async Task<CategoryId> NewCategoryAsync()
    {
        var (rsp, category) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(
            new(Unique("Category"), null));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        return category.Id;
    }

    /// <summary>A Published Red/Blue mug: Red costs 120_000 with 5 in stock, Blue 130_000 with 2.</summary>
    private async Task<ProductResponse> NewPublishedMugAsync(TestMerchant merchant)
    {
        var (rsp, product) = await merchant.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(
            new(Unique("Mug"), null, await NewCategoryAsync(), [Colour],
            [
                new(["Red"], 120_000, 5, 350),
                new(["Blue"], 130_000, 2, 360),
            ]));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        var (publishRsp, published) = await merchant.Client
            .POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        publishRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return published;
    }

    private static VariantId Red(ProductResponse product) => product.Variants.Single(v => v.OptionValues[0] == "Red").Id;
    private static VariantId Blue(ProductResponse product) => product.Variants.Single(v => v.OptionValues[0] == "Blue").Id;

    private static async Task<CartResponse> GetCartAsync(HttpClient buyer)
    {
        var (rsp, cart) = await buyer.GETAsync<GetCartEndpoint, CartResponse>();
        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return cart;
    }

    private static async Task<CartResponse> AddAsync(HttpClient buyer, VariantId variantId, int quantity = 1)
    {
        var (rsp, cart) = await buyer.POSTAsync<AddCartLineEndpoint, AddCartLineRequest, CartResponse>(new(variantId, quantity));
        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return cart;
    }

    private static CartLineResponse LineOf(CartResponse cart, VariantId variantId) => cart.Lines.Single(l => l.VariantId == variantId);

    /// <summary>
    /// Reads until <paramref name="until"/> holds, for what an event changes in the background; fails after
    /// <see cref="EventTimeout"/> with the last value read.
    /// </summary>
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
    public async Task New_buyer_has_an_empty_cart()
    {
        var cart = await GetCartAsync(App.ClientFor(TestActor.Buyer()));

        cart.Lines.ShouldBeEmpty();
        cart.ItemsTotal.ShouldBe(Money.From(0));
    }

    [Fact]
    public async Task Cart_needs_a_signed_in_account()
    {
        var (rsp, _) = await App.Client.GETAsync<GetCartEndpoint, CartResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Buyer_adds_variants_and_sees_them_live_with_the_items_total()
    {
        var merchant = await NewMerchantAsync();
        var mug = await NewPublishedMugAsync(merchant);
        var buyer = App.ClientFor(TestActor.Buyer());

        await AddAsync(buyer, Red(mug), 2);
        await AddAsync(buyer, Blue(mug));
        var cart = await AddAsync(buyer, Red(mug));

        cart.Lines.Select(l => l.VariantId).ShouldBe([Red(mug), Blue(mug)]);
        var red = cart.Lines[0];
        red.Quantity.ShouldBe(3);
        red.ProductId.ShouldBe(mug.Id);
        red.ProductName.ShouldBe(mug.Name);
        red.OptionValues.ShouldBe(["Red"]);
        red.ShopId.ShouldBe(merchant.Id);
        red.ShopName.ShouldBe(merchant.ShopName);
        red.UnitPrice.ShouldBe(Money.From(120_000));
        red.LineTotal.ShouldBe(Money.From(360_000));
        red.Available.ShouldBe(5);
        red.Flags.ShouldBeEmpty();
        cart.ItemsTotal.ShouldBe(Money.From(490_000));

        (await GetCartAsync(buyer)).ItemsTotal.ShouldBe(Money.From(490_000));
    }

    [Fact]
    public async Task Cart_persists_across_sessions_and_no_one_else_sees_it()
    {
        var mug = await NewPublishedMugAsync(await NewMerchantAsync());
        var buyer = TestActor.Buyer();
        await AddAsync(App.ClientFor(buyer), Red(mug));

        // A new token for the same subject is a new session.
        (await GetCartAsync(App.ClientFor(buyer))).Lines.Select(l => l.VariantId).ShouldBe([Red(mug)]);
        (await GetCartAsync(App.ClientFor(TestActor.Buyer()))).Lines.ShouldBeEmpty();
    }

    [Fact]
    public async Task Buyer_changes_a_quantity_and_removes_a_line()
    {
        var mug = await NewPublishedMugAsync(await NewMerchantAsync());
        var buyer = App.ClientFor(TestActor.Buyer());
        await AddAsync(buyer, Red(mug));
        await AddAsync(buyer, Blue(mug));

        var (updateRsp, updated) = await buyer.PUTAsync<UpdateCartLineEndpoint, UpdateCartLineRequest, CartResponse>(new(Red(mug), 4));
        updateRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        LineOf(updated, Red(mug)).Quantity.ShouldBe(4);

        var (removeRsp, removed) = await buyer.DELETEAsync<RemoveCartLineEndpoint, RemoveCartLineRequest, CartResponse>(new(Blue(mug)));
        removeRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        removed.Lines.Select(l => l.VariantId).ShouldBe([Red(mug)]);
        (await GetCartAsync(buyer)).Lines.Select(l => (l.VariantId, l.Quantity)).ShouldBe([(Red(mug), 4)]);
    }

    [Fact]
    public async Task Changing_or_removing_a_line_the_cart_does_not_have_is_not_found()
    {
        var buyer = App.ClientFor(TestActor.Buyer());

        (await buyer.PUTAsync<UpdateCartLineEndpoint, UpdateCartLineRequest, CartResponse>(new(VariantId.New(), 1)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await buyer.DELETEAsync<RemoveCartLineEndpoint, RemoveCartLineRequest, CartResponse>(new(VariantId.New())))
            .Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Adding_a_variant_that_is_not_buyable_is_not_found()
    {
        var merchant = await NewMerchantAsync();
        var (_, draft) = await merchant.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(
            new(Unique("Draft"), null, await NewCategoryAsync(), null, [new(null, 50_000, 5, 100)]));
        var buyer = App.ClientFor(TestActor.Buyer());

        (await buyer.POSTAsync<AddCartLineEndpoint, AddCartLineRequest, CartResponse>(new(draft.Variants[0].Id, 1)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await buyer.POSTAsync<AddCartLineEndpoint, AddCartLineRequest, CartResponse>(new(VariantId.New(), 1)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetCartAsync(buyer)).Lines.ShouldBeEmpty();
    }

    [Fact]
    public async Task Quantity_outside_1_to_99_is_a_validation_error()
    {
        var mug = await NewPublishedMugAsync(await NewMerchantAsync());
        var buyer = App.ClientFor(TestActor.Buyer());

        var (addRsp, addProblem) = await buyer.POSTAsync<AddCartLineEndpoint, AddCartLineRequest, ProblemDetails>(new(Red(mug), 0));
        addRsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        addProblem.Errors.Select(e => e.Name).ShouldBe(["quantity"]);

        await AddAsync(buyer, Red(mug), 98);
        var (overRsp, _) = await buyer.POSTAsync<AddCartLineEndpoint, AddCartLineRequest, ProblemDetails>(new(Red(mug), 2));
        overRsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var (updateRsp, updateProblem) = await buyer.PUTAsync<UpdateCartLineEndpoint, UpdateCartLineRequest, ProblemDetails>(new(Red(mug), 100));
        updateRsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        updateProblem.Errors.Select(e => e.Name).ShouldBe(["quantity"]);

        LineOf(await GetCartAsync(buyer), Red(mug)).Quantity.ShouldBe(98);
    }

    [Fact]
    public async Task Cart_holds_at_most_50_different_variants()
    {
        var merchant = await NewMerchantAsync();
        var options = new ProductOption("Number", [.. Enumerable.Range(1, 17).Select(i => $"{i}")]);
        var sizes = new ProductOption("Size", ["S", "M", "L"]);
        var (_, product) = await merchant.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(
            new(Unique("Sticker"), null, await NewCategoryAsync(), [options, sizes],
            [.. options.Values.SelectMany(n => sizes.Values.Select(s => new VariantRequest([n, s], 1_000, 10, 10)))]));
        await merchant.Client.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        var buyer = App.ClientFor(TestActor.Buyer());

        foreach (var variant in product.Variants.Take(Cart.MaxLines)) await AddAsync(buyer, variant.Id);
        var (rsp, _) = await buyer.POSTAsync<AddCartLineEndpoint, AddCartLineRequest, ProblemDetails>(new(product.Variants[Cart.MaxLines].Id, 1));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await GetCartAsync(buyer)).Lines.Count.ShouldBe(Cart.MaxLines);
    }

    [Fact]
    public async Task Quantity_above_available_is_added_but_flagged()
    {
        var mug = await NewPublishedMugAsync(await NewMerchantAsync());
        var buyer = App.ClientFor(TestActor.Buyer());

        var cart = await AddAsync(buyer, Blue(mug), 3);

        LineOf(cart, Blue(mug)).Flags.ShouldBe([CartLineFlag.InsufficientStock]);
    }

    [Fact]
    public async Task Unpublished_product_stays_flagged_until_published_again()
    {
        var merchant = await NewMerchantAsync();
        var mug = await NewPublishedMugAsync(merchant);
        var buyer = App.ClientFor(TestActor.Buyer());
        await AddAsync(buyer, Red(mug));

        await merchant.Client.POSTAsync<UnpublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(mug.Id));
        var hidden = await GetCartAsync(buyer);
        LineOf(hidden, Red(mug)).Flags.ShouldBe([CartLineFlag.Unavailable]);
        hidden.ItemsTotal.ShouldBe(Money.From(0));

        await merchant.Client.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(mug.Id));
        LineOf(await GetCartAsync(buyer), Red(mug)).Flags.ShouldBeEmpty();
    }

    [Fact]
    public async Task Suspended_merchants_variants_stay_flagged_until_reinstated()
    {
        var merchant = await NewMerchantAsync();
        var mug = await NewPublishedMugAsync(merchant);
        var buyer = App.ClientFor(TestActor.Buyer());
        await AddAsync(buyer, Red(mug));

        (await _operator.POSTAsync<SuspendMerchantEndpoint, ChangeMerchantStatusRequest, OperatorMerchantResponse>(new(merchant.Id)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await EventuallyAsync(() => GetCartAsync(buyer), c => LineOf(c, Red(mug)).Flags.SequenceEqual([CartLineFlag.Unavailable]));

        (await _operator.POSTAsync<ReinstateMerchantEndpoint, ChangeMerchantStatusRequest, OperatorMerchantResponse>(new(merchant.Id)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await EventuallyAsync(() => GetCartAsync(buyer), c => LineOf(c, Red(mug)).Flags.Count == 0);
    }

    [Fact]
    public async Task Variant_removed_by_a_product_edit_stays_in_the_cart_flagged()
    {
        var merchant = await NewMerchantAsync();
        var mug = await NewPublishedMugAsync(merchant);
        var buyer = App.ClientFor(TestActor.Buyer());
        await AddAsync(buyer, Blue(mug));

        var (editRsp, _) = await merchant.Client.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProductResponse>(
            new(mug.Id, mug.Name, null, mug.CategoryId, [new("Colour", ["Red"])], [new(["Red"], 120_000, 5, 350)]));
        editRsp.StatusCode.ShouldBe(HttpStatusCode.OK);

        var line = LineOf(await GetCartAsync(buyer), Blue(mug));
        line.Quantity.ShouldBe(1);
        line.ProductName.ShouldBeNull();
        line.Flags.ShouldBe([CartLineFlag.Unavailable]);
    }

    [Fact]
    public async Task Merchants_own_variants_are_flagged()
    {
        var merchant = await NewMerchantAsync();
        var mug = await NewPublishedMugAsync(merchant);

        var cart = await AddAsync(merchant.Client, Red(mug));

        LineOf(cart, Red(mug)).Flags.ShouldBe([CartLineFlag.OwnMerchant]);
    }

    [Fact]
    public async Task Concurrent_edits_of_one_cart_give_a_conflict()
    {
        var mug = await NewPublishedMugAsync(await NewMerchantAsync());
        var actor = TestActor.Buyer();
        var buyer = App.ClientFor(actor);
        await AddAsync(buyer, Red(mug));
        var ct = TestContext.Current.CancellationToken;

        // Hold the Cart's row, so both edits read the same version and then wait to write it; when it is let go, the
        // first edit saves and the second finds the version changed.
        var connectionString = App.Services.GetRequiredService<IConfiguration>().GetConnectionString("micro-commerce-db");
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await using (var lockCart = new NpgsqlCommand(
            """
            SELECT c."AccountId" FROM ordering."Carts" c
            JOIN accounts."Accounts" a ON a."Id" = c."AccountId"
            WHERE a."Subject" = @subject FOR UPDATE OF c
            """, connection, transaction))
        {
            lockCart.Parameters.AddWithValue("subject", actor.Subject);
            (await lockCart.ExecuteScalarAsync(ct)).ShouldNotBeNull();
        }

        var edits = new[] { 2, 3 }.Select(quantity =>
            buyer.PUTAsync<UpdateCartLineEndpoint, UpdateCartLineRequest, CartResponse>(new(Red(mug), quantity))).ToList();
        await EventuallyAsync(() => LockWaits.CountBlockedByAsync(connectionString!, connection, ct), count => count >= 2);
        await transaction.RollbackAsync(ct);

        var statuses = (await Task.WhenAll(edits)).Select(r => r.Response.StatusCode).ToList();
        statuses.ShouldBe([HttpStatusCode.OK, HttpStatusCode.Conflict], ignoreOrder: true);
    }
}
