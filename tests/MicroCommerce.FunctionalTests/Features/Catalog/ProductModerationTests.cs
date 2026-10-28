using System.Diagnostics;
using System.Net;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

public class ProductModerationTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");

    /// <summary>How long the Catalog may take to see a Merchant event; it normally takes milliseconds.</summary>
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(15);

    private readonly HttpClient _operator = App.ClientFor(TestActor.PlatformOperator());

    // The database is shared across the test run, so every test makes its own Merchants, Categories and Products.
    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private sealed record TestMerchant(HttpClient Client, MerchantResponse Merchant);

    /// <summary>A fresh Merchant, once its Shop has reached the Catalog.</summary>
    private async Task<TestMerchant> NewMerchantAsync()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var (rsp, merchant) = await client.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
            new(Unique("Shop"), Address));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);

        await EventuallyAsync(
            async () => (await App.Client.GETAsync<GetShopPageEndpoint, GetShopPageRequest, ShopResponse>(new(merchant.Id))).Response.StatusCode,
            status => status == HttpStatusCode.OK);
        return new(client, merchant);
    }

    private async Task<ProductResponse> NewProductAsync(TestMerchant merchant, bool publish = true)
    {
        var (categoryRsp, category) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(
            new(Unique("Category"), null));
        categoryRsp.StatusCode.ShouldBe(HttpStatusCode.Created);

        var (rsp, product) = await merchant.Client.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(
            new(Unique("Mug"), "A sturdy stoneware mug.", category.Id, null, [new(null, 120_000, 5, 350)]));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        if (!publish) return product;

        var (publishRsp, published) = await merchant.Client.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        publishRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return published;
    }

    private Task<TestResult<OperatorProductResponse>> RemoveAsync(ProductId id, HttpClient? client = null) =>
        (client ?? _operator).POSTAsync<RemoveProductEndpoint, RemoveProductRequest, OperatorProductResponse>(new(id));

    private Task<TestResult<OperatorProductResponse>> ViewAsync(ProductId id, HttpClient? client = null) =>
        (client ?? _operator).GETAsync<GetOperatorProductEndpoint, GetOperatorProductRequest, OperatorProductResponse>(new(id));

    private Task<TestResult<StorefrontProductResponse>> StorefrontDetailAsync(ProductId id) =>
        App.Client.GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(new(id));

    private async Task<PagedResponse<StorefrontProductSummaryResponse>> StorefrontListAsync(ListStorefrontProductsRequest request)
    {
        var (rsp, page) = await App.Client
            .GETAsync<ListStorefrontProductsEndpoint, ListStorefrontProductsRequest, PagedResponse<StorefrontProductSummaryResponse>>(request);
        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        return page;
    }

    /// <summary>Reads until <paramref name="until"/> holds; fails after <see cref="EventTimeout"/> with the last value read.</summary>
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

    private static void ShouldBeRefusedAsRemoved(HttpResponseMessage rsp, ProblemDetails problem)
    {
        rsp.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.Errors.ShouldHaveSingleItem().Reason.ShouldBe(Product.RemovedMessage);
    }

    [Fact]
    public async Task Operator_views_any_product_in_any_status_with_its_merchant()
    {
        var merchant = await NewMerchantAsync();
        var draft = await NewProductAsync(merchant, publish: false);

        var (rsp, product) = await ViewAsync(draft.Id);

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        product.ShouldBeEquivalentTo(new OperatorProductResponse(
            draft.Id, merchant.Merchant.Id, merchant.Merchant.ShopName.Value, draft.Name, draft.Description, draft.CategoryId,
            ProductStatus.Draft, draft.Options, draft.Variants, draft.CreatedAt));
    }

    [Fact]
    public async Task Operator_removes_a_product()
    {
        var merchant = await NewMerchantAsync();
        var published = await NewProductAsync(merchant);

        var (rsp, removed) = await RemoveAsync(published.Id);
        var (_, viewed) = await ViewAsync(published.Id);
        var (againRsp, again) = await RemoveAsync(published.Id);

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        (removed.Id, removed.MerchantId, removed.Status).ShouldBe((published.Id, merchant.Merchant.Id, ProductStatus.Removed));
        viewed.Status.ShouldBe(ProductStatus.Removed);
        againRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        again.Status.ShouldBe(ProductStatus.Removed);
    }

    [Fact]
    public async Task Unknown_product_is_not_found_by_operators()
    {
        (await ViewAsync(ProductId.New())).Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await RemoveAsync(ProductId.New())).Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Product_moderation_needs_the_operator_role()
    {
        var merchant = await NewMerchantAsync();
        var product = await NewProductAsync(merchant);

        // Not even the Product's own Merchant may use it.
        (await ViewAsync(product.Id, merchant.Client)).Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await RemoveAsync(product.Id, merchant.Client)).Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ViewAsync(product.Id, App.Client)).Response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await RemoveAsync(product.Id, App.Client)).Response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ViewAsync(product.Id)).Result.Status.ShouldBe(ProductStatus.Published);
    }

    [Fact]
    public async Task Merchant_can_no_longer_change_a_removed_product()
    {
        var merchant = await NewMerchantAsync();
        var product = await NewProductAsync(merchant);
        await RemoveAsync(product.Id);

        var (publishRsp, publishProblem) = await merchant.Client
            .POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProblemDetails>(new(product.Id));
        var (unpublishRsp, unpublishProblem) = await merchant.Client
            .POSTAsync<UnpublishProductEndpoint, ChangeProductStatusRequest, ProblemDetails>(new(product.Id));
        var (updateRsp, updateProblem) = await merchant.Client.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProblemDetails>(
            new(product.Id, Unique("Mug"), null, product.CategoryId, null, [new(null, 99_000, 1, 300)]));

        ShouldBeRefusedAsRemoved(publishRsp, publishProblem);
        ShouldBeRefusedAsRemoved(unpublishRsp, unpublishProblem);
        ShouldBeRefusedAsRemoved(updateRsp, updateProblem);

        // The Merchant still sees it, unchanged, marked Removed.
        var (getRsp, mine) = await merchant.Client.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(new(product.Id));
        getRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        (mine.Name, mine.Status).ShouldBe((product.Name, ProductStatus.Removed));
        var (_, list) = await merchant.Client.GETAsync<ListProductsEndpoint, IReadOnlyList<ProductSummaryResponse>>();
        list.ShouldHaveSingleItem().Status.ShouldBe(ProductStatus.Removed);
    }

    [Fact]
    public async Task Removed_products_leave_the_storefront()
    {
        var merchant = await NewMerchantAsync();
        var product = await NewProductAsync(merchant);
        var kept = await NewProductAsync(merchant);
        var search = new ListStorefrontProductsRequest { Query = product.Name };
        (await StorefrontListAsync(search)).Items.Select(p => p.Id).ShouldBe([product.Id]);
        var (cachedRsp, _) = await StorefrontDetailAsync(product.Id);
        cachedRsp.StatusCode.ShouldBe(HttpStatusCode.OK);

        await RemoveAsync(product.Id);

        // A cached detail entry is evicted too.
        (await StorefrontDetailAsync(product.Id)).Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await StorefrontListAsync(new() { ShopId = merchant.Merchant.Id })).Items.Select(p => p.Id).ShouldBe([kept.Id]);
        (await StorefrontListAsync(search)).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Removing_a_draft_keeps_it_off_the_storefront()
    {
        var merchant = await NewMerchantAsync();
        var draft = await NewProductAsync(merchant, publish: false);

        await RemoveAsync(draft.Id);
        var (publishRsp, _) = await merchant.Client
            .POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProblemDetails>(new(draft.Id));

        publishRsp.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await StorefrontDetailAsync(draft.Id)).Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
