using System.Diagnostics;
using System.Net;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

public class StorefrontEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");
    private static readonly ProductOption Colour = new("Colour", ["Red", "Blue"]);
    private static readonly ProductOption Size = new("Size", ["S", "M"]);

    /// <summary>How long the Catalog may take to see a Merchant event; it normally takes milliseconds.</summary>
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(15);

    private readonly HttpClient _operator = App.ClientFor(TestActor.PlatformOperator());

    // Buyers browse anonymously.
    private HttpClient Buyer => App.Client;

    // The database is shared across the test run, so every test makes its own Merchants, Categories and Products.
    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private sealed record TestMerchant(HttpClient Client, MerchantResponse Merchant)
    {
        public MerchantId Id => Merchant.Id;
    }

    /// <summary>A fresh Merchant, once its Shop has reached the Catalog.</summary>
    private async Task<TestMerchant> NewMerchantAsync(string? description = null)
    {
        var client = App.ClientFor(TestActor.Buyer());
        var (rsp, merchant) = await client.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
            new(Unique("Shop"), Address));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);

        if (description is not null)
            (_, merchant) = await client.PUTAsync<UpdateShopEndpoint, UpdateShopRequest, MerchantResponse>(
                new(merchant.ShopName.Value, description, Address));

        await ShopPageAsync(merchant.Id, until: s => s?.Description == merchant.Description);
        return new(client, merchant);
    }

    private async Task<CategoryId> NewCategoryAsync()
    {
        var (rsp, category) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(
            new(Unique("Category"), null));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        return category.Id;
    }

    private static CreateProductRequest NewMug(CategoryId categoryId, int stock = 5) =>
        new(Unique("Mug"), "A sturdy stoneware mug.", categoryId, null, [new(null, 120_000, stock, 350)]);

    /// <summary>A Colour × Size T-shirt: Red / S, Red / M, Blue / S, Blue / M, priced 100_000 to 103_000; Red / S is out of stock.</summary>
    private static CreateProductRequest NewShirt(CategoryId categoryId) =>
        new(Unique("T-Shirt"), "Cotton.", categoryId, [Colour, Size],
        [
            new(["Red", "S"], 100_000, 0, 200),
            new(["Red", "M"], 101_000, 1, 210),
            new(["Blue", "S"], 102_000, 2, 220),
            new(["Blue", "M"], 103_000, 3, 230),
        ]);

    private static async Task<ProductResponse> CreateAsync(TestMerchant merchant, CreateProductRequest request)
    {
        var (rsp, product) = await merchant.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(request);
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        return product;
    }

    private static async Task<ProductResponse> PublishAsync(TestMerchant merchant, ProductId id)
    {
        var (rsp, product) = await merchant.Client.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(id));
        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return product;
    }

    private static async Task<ProductResponse> CreatePublishedAsync(TestMerchant merchant, CreateProductRequest request) =>
        await PublishAsync(merchant, (await CreateAsync(merchant, request)).Id);

    private async Task<PagedResponse<StorefrontProductSummaryResponse>> ListAsync(ListStorefrontProductsRequest request)
    {
        var (rsp, page) = await Buyer
            .GETAsync<ListStorefrontProductsEndpoint, ListStorefrontProductsRequest, PagedResponse<StorefrontProductSummaryResponse>>(request);
        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return page;
    }

    private Task<TestResult<StorefrontProductResponse>> GetAsync(ProductId id) =>
        Buyer.GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(new(id));

    /// <summary>The Shop page header, polled until <paramref name="until"/> holds: Shops reach the Catalog by event.</summary>
    private Task<ShopResponse?> ShopPageAsync(MerchantId id, Func<ShopResponse?, bool> until) =>
        EventuallyAsync(async () =>
        {
            var (rsp, shop) = await Buyer.GETAsync<GetShopPageEndpoint, GetShopPageRequest, ShopResponse>(new(id));
            return rsp.StatusCode == HttpStatusCode.OK ? shop : null;
        }, until);

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
    public async Task Opening_a_merchant_brings_its_shop_to_the_storefront()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var (_, merchant) = await client.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
            new(Unique("Shop"), Address));

        var shop = await ShopPageAsync(merchant.Id, until: s => s is not null);

        shop.ShouldBe(new ShopResponse(merchant.Id, merchant.ShopName.Value, null));
    }

    [Fact]
    public async Task Shop_page_follows_profile_changes()
    {
        var merchant = await NewMerchantAsync();
        var newName = Unique("Renamed");

        await merchant.Client.PUTAsync<UpdateShopEndpoint, UpdateShopRequest, MerchantResponse>(
            new(newName, "Mugs and teapots.", Address));

        var shop = await ShopPageAsync(merchant.Id, until: s => s?.ShopName == newName);
        shop.ShouldBe(new ShopResponse(merchant.Id, newName, "Mugs and teapots."));
    }

    [Fact]
    public async Task Unknown_shop_is_not_found()
    {
        var (rsp, _) = await Buyer.GETAsync<GetShopPageEndpoint, GetShopPageRequest, ShopResponse>(new(MerchantId.New()));

        rsp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Published_products_are_listed_with_price_range_stock_and_shop()
    {
        var merchant = await NewMerchantAsync();
        var category = await NewCategoryAsync();
        var shirt = await CreatePublishedAsync(merchant, NewShirt(category));
        var soldOut = await CreatePublishedAsync(merchant, NewMug(category, stock: 0));

        var page = await ListAsync(new() { ShopId = merchant.Id });

        page.TotalCount.ShouldBe(2);
        page.Items.ShouldBe(
        [
            new(soldOut.Id, soldOut.Name, category, Money.From(120_000), Money.From(120_000), false, merchant.Id, merchant.Merchant.ShopName.Value),
            new(shirt.Id, shirt.Name, category, Money.From(100_000), Money.From(103_000), true, merchant.Id, merchant.Merchant.ShopName.Value),
        ]);
    }

    [Fact]
    public async Task List_spans_every_merchant()
    {
        var category = await NewCategoryAsync();
        var first = await CreatePublishedAsync(await NewMerchantAsync(), NewMug(category));
        var second = await CreatePublishedAsync(await NewMerchantAsync(), NewMug(category));

        // Other tests publish Products too, but ours are among the newest.
        var page = await ListAsync(new() { PageSize = ListStorefrontProductsRequest.MaxPageSize });

        page.Items.Select(p => p.Id).ShouldContain(first.Id);
        page.Items.Select(p => p.Id).ShouldContain(second.Id);
    }

    [Fact]
    public async Task List_is_paginated_newest_first()
    {
        var merchant = await NewMerchantAsync();
        var category = await NewCategoryAsync();
        var oldest = await CreatePublishedAsync(merchant, NewMug(category));
        var middle = await CreatePublishedAsync(merchant, NewMug(category));
        var newest = await CreatePublishedAsync(merchant, NewMug(category));

        var first = await ListAsync(new() { ShopId = merchant.Id, Page = 1, PageSize = 2 });
        var second = await ListAsync(new() { ShopId = merchant.Id, Page = 2, PageSize = 2 });
        var beyond = await ListAsync(new() { ShopId = merchant.Id, Page = 3, PageSize = 2 });

        first.Items.Select(p => p.Id).ShouldBe([newest.Id, middle.Id]);
        (first.Page, first.PageSize, first.TotalCount).ShouldBe((1, 2, 3));
        second.Items.Select(p => p.Id).ShouldBe([oldest.Id]);
        (second.Page, second.TotalCount).ShouldBe((2, 3));
        beyond.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Invalid_paging_is_a_bad_request()
    {
        var (rsp, problem) = await Buyer.GETAsync<ListStorefrontProductsEndpoint, ListStorefrontProductsRequest, ProblemDetails>(
            new() { Page = 0, PageSize = ListStorefrontProductsRequest.MaxPageSize + 1 });

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["page", "pageSize"], ignoreOrder: true);
    }

    [Fact]
    public async Task Product_detail_shows_description_category_options_and_variants()
    {
        var merchant = await NewMerchantAsync();
        var category = await NewCategoryAsync();
        var shirt = await CreatePublishedAsync(merchant, NewShirt(category));

        var (rsp, product) = await GetAsync(shirt.Id);

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        (product.Id, product.Name, product.Description, product.CategoryId).ShouldBe((shirt.Id, shirt.Name, "Cotton.", category));
        (product.ShopId, product.ShopName).ShouldBe((merchant.Id, merchant.Merchant.ShopName.Value));
        product.Options.Select(o => $"{o.Name}: {string.Join(", ", o.Values)}").ShouldBe(["Colour: Red, Blue", "Size: S, M"]);
        product.Variants.Select(v => (v.Id, v.Price, v.Stock)).ShouldBe(shirt.Variants.Select(v => (v.Id, v.Price, v.Stock)));
        product.Variants.Select(v => string.Join(" / ", v.OptionValues)).ShouldBe(["Red / S", "Red / M", "Blue / S", "Blue / M"]);
    }

    [Fact]
    public async Task Draft_products_are_neither_listed_nor_found()
    {
        var merchant = await NewMerchantAsync();
        var category = await NewCategoryAsync();
        var draft = await CreateAsync(merchant, NewMug(category));
        var published = await CreatePublishedAsync(merchant, NewMug(category));

        var page = await ListAsync(new() { ShopId = merchant.Id });
        var (draftRsp, _) = await GetAsync(draft.Id);

        page.Items.Select(p => p.Id).ShouldBe([published.Id]);
        draftRsp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unpublished_products_leave_the_storefront()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreatePublishedAsync(merchant, NewMug(await NewCategoryAsync()));
        (await GetAsync(product.Id)).Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        await merchant.Client.POSTAsync<UnpublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));

        (await GetAsync(product.Id)).Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ListAsync(new() { ShopId = merchant.Id })).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Unknown_product_is_not_found()
    {
        var (rsp, _) = await GetAsync(ProductId.New());

        rsp.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Product_detail_is_cached()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreatePublishedAsync(merchant, NewMug(await NewCategoryAsync()));

        var (first, firstBody) = await GetAsync(product.Id);
        var (second, secondBody) = await GetAsync(product.Id);

        first.Headers.GetValues("X-Cache").ShouldBe(["MISS"]);
        second.Headers.GetValues("X-Cache").ShouldBe(["HIT"]);
        secondBody.ShouldBeEquivalentTo(firstBody);
    }

    [Fact]
    public async Task Editing_a_product_evicts_its_cached_detail()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreatePublishedAsync(merchant, NewMug(await NewCategoryAsync()));
        await GetAsync(product.Id);
        var newName = Unique("Mug");

        await merchant.Client.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProductResponse>(
            new(product.Id, newName, null, product.CategoryId, null, [new(null, 99_000, 7, 300)]));
        var (rsp, edited) = await GetAsync(product.Id);

        rsp.Headers.GetValues("X-Cache").ShouldBe(["MISS"]);
        edited.Name.ShouldBe(newName);
        edited.Description.ShouldBeNull();
        var variant = edited.Variants.ShouldHaveSingleItem();
        (variant.Id, variant.Price, variant.Stock).ShouldBe((product.Variants[0].Id, Money.From(99_000), 7));
        variant.OptionValues.ShouldBeEmpty();
    }

    [Fact]
    public async Task Republishing_a_product_serves_it_fresh()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreatePublishedAsync(merchant, NewMug(await NewCategoryAsync()));
        await GetAsync(product.Id);

        await merchant.Client.POSTAsync<UnpublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        await PublishAsync(merchant, product.Id);
        var (rsp, _) = await GetAsync(product.Id);

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        rsp.Headers.GetValues("X-Cache").ShouldBe(["MISS"]);
    }

    [Fact]
    public async Task Renaming_the_shop_reaches_its_products()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreatePublishedAsync(merchant, NewMug(await NewCategoryAsync()));
        await GetAsync(product.Id);
        var newName = Unique("Renamed");

        await merchant.Client.PUTAsync<UpdateShopEndpoint, UpdateShopRequest, MerchantResponse>(new(newName, null, Address));

        var detail = await EventuallyAsync(async () => (await GetAsync(product.Id)).Result, p => p.ShopName == newName);
        detail.ShopId.ShouldBe(merchant.Id);
        (await ListAsync(new() { ShopId = merchant.Id })).Items.ShouldHaveSingleItem().ShopName.ShouldBe(newName);
    }
}
