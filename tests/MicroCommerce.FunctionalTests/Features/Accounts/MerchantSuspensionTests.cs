using System.Diagnostics;
using System.Net;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.FunctionalTests.Features.Accounts;

public class MerchantSuspensionTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");
    private static readonly TimeSpan EventTimeout = TimeSpan.FromSeconds(15);
    private readonly HttpClient _operator = App.ClientFor(TestActor.PlatformOperator());

    private sealed record TestMerchant(HttpClient Client, MerchantResponse Merchant)
    {
        public MerchantId Id => Merchant.Id;
    }

    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    private static CreateProductRequest NewProduct(CategoryId categoryId, string name) =>
        new(name, "Suspension test product.", categoryId, null, [new(null, 120_000, 5, 350)]);

    private async Task<TestMerchant> NewMerchantAsync()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var (response, merchant) = await client.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
            new(Unique("Shop"), Address));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        await WaitForShopStatusAsync(merchant.Id, HttpStatusCode.OK);
        return new(client, merchant);
    }

    private async Task<CategoryId> NewCategoryAsync()
    {
        var (response, category) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(
            new(Unique("Category"), null));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return category.Id;
    }

    private Task<HttpStatusCode> ShopStatusAsync(MerchantId id) => ReadShopStatusAsync(id);

    private async Task<HttpStatusCode> ReadShopStatusAsync(MerchantId id)
    {
        var (response, _) = await App.Client.GETAsync<GetShopPageEndpoint, GetShopPageRequest, ShopResponse>(new(id));
        return response.StatusCode;
    }

    private Task WaitForShopStatusAsync(MerchantId id, HttpStatusCode status) =>
        EventuallyAsync(() => ShopStatusAsync(id), result => result == status);

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
    public async Task Operator_can_list_suspend_and_reinstate_merchants_and_storefront_visibility_tracks_events()
    {
        var merchant = await NewMerchantAsync();
        var categoryId = await NewCategoryAsync();
        var searchWord = string.Concat(Guid.NewGuid().ToString("N").Select(c => (char)('a' + Convert.ToInt32($"{c}", 16))));
        var (createResponse, product) = await merchant.Client
            .POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(NewProduct(categoryId, $"Mug {searchWord}"));
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var (publishResponse, _) = await merchant.Client
            .POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        publishResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var (firstDetail, _) = await App.Client
            .GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(new(product.Id));
        var (cachedDetail, _) = await App.Client
            .GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(new(product.Id));
        firstDetail.Headers.GetValues("X-Cache").ShouldBe(["MISS"]);
        cachedDetail.Headers.GetValues("X-Cache").ShouldBe(["HIT"]);

        var (suspendResponse, suspended) = await _operator
            .POSTAsync<SuspendMerchantEndpoint, ChangeMerchantStatusRequest, OperatorMerchantResponse>(new(merchant.Id));
        suspendResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        suspended.Status.ShouldBe(MerchantStatus.Suspended);
        await WaitForShopStatusAsync(merchant.Id, HttpStatusCode.NotFound);

        var (listResponse, operatorMerchants) =
            await _operator.GETAsync<ListMerchantsEndpoint, IReadOnlyList<OperatorMerchantResponse>>();
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        operatorMerchants.Single(m => m.Id == merchant.Id).Status.ShouldBe(MerchantStatus.Suspended);

        var storefrontList = await App.Client.GETAsync<ListStorefrontProductsEndpoint, ListStorefrontProductsRequest,
            PagedResponse<StorefrontProductSummaryResponse>>(new() { ShopId = merchant.Id });
        storefrontList.Result.TotalCount.ShouldBe(0);
        var search = await App.Client.GETAsync<ListStorefrontProductsEndpoint, ListStorefrontProductsRequest,
            PagedResponse<StorefrontProductSummaryResponse>>(new() { Query = searchWord });
        search.Result.TotalCount.ShouldBe(0);
        (await App.Client.GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(new(product.Id)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var (reinstateResponse, reinstated) = await _operator
            .POSTAsync<ReinstateMerchantEndpoint, ChangeMerchantStatusRequest, OperatorMerchantResponse>(new(merchant.Id));
        reinstateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        reinstated.Status.ShouldBe(MerchantStatus.Active);
        await WaitForShopStatusAsync(merchant.Id, HttpStatusCode.OK);

        var restoredList = await App.Client.GETAsync<ListStorefrontProductsEndpoint, ListStorefrontProductsRequest,
            PagedResponse<StorefrontProductSummaryResponse>>(new() { ShopId = merchant.Id });
        restoredList.Result.Items.Select(p => p.Id).ShouldContain(product.Id);
        var restoredSearch = await App.Client.GETAsync<ListStorefrontProductsEndpoint, ListStorefrontProductsRequest,
            PagedResponse<StorefrontProductSummaryResponse>>(new() { Query = searchWord });
        restoredSearch.Result.Items.Select(p => p.Id).ShouldContain(product.Id);
        var (restoredDetail, _) = await App.Client
            .GETAsync<GetStorefrontProductEndpoint, GetStorefrontProductRequest, StorefrontProductResponse>(new(product.Id));
        restoredDetail.StatusCode.ShouldBe(HttpStatusCode.OK);
        restoredDetail.Headers.GetValues("X-Cache").ShouldBe(["MISS"]);
    }

    [Fact]
    public async Task Suspended_merchant_can_read_merchant_centre_but_all_writes_are_forbidden()
    {
        var merchant = await NewMerchantAsync();
        var categoryId = await NewCategoryAsync();
        var (draftResponse, draft) = await merchant.Client
            .POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(NewProduct(categoryId, Unique("Draft")));
        draftResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var (publishedResponse, published) = await merchant.Client
            .POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(NewProduct(categoryId, Unique("Published")));
        publishedResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await merchant.Client.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(published.Id)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var (suspendResponse, _) = await _operator
            .POSTAsync<SuspendMerchantEndpoint, ChangeMerchantStatusRequest, OperatorMerchantResponse>(new(merchant.Id));
        suspendResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        await WaitForShopStatusAsync(merchant.Id, HttpStatusCode.NotFound);

        var (shopResponse, shop) = await merchant.Client.GETAsync<GetShopEndpoint, MerchantResponse>();
        shopResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        shop.Status.ShouldBe(MerchantStatus.Suspended);
        (await merchant.Client.GETAsync<ListProductsEndpoint, IReadOnlyList<ProductSummaryResponse>>()).Response.StatusCode
            .ShouldBe(HttpStatusCode.OK);
        (await merchant.Client.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(new(draft.Id)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var (editShopResponse, editShopProblem) = await merchant.Client
            .PUTAsync<UpdateShopEndpoint, UpdateShopRequest, ProblemDetails>(new(Unique("Renamed"), null, Address));
        editShopResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        editShopProblem.Status.ShouldBe(403);

        var create = await merchant.Client
            .POSTAsync<CreateProductEndpoint, CreateProductRequest, ProblemDetails>(NewProduct(categoryId, Unique("New")));
        create.Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        create.Result.Status.ShouldBe(403);

        var editProduct = await merchant.Client.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProblemDetails>(
            new(draft.Id, Unique("Edited"), null, categoryId, null, [new(null, 120_000, 5, 350)]));
        editProduct.Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        editProduct.Result.Status.ShouldBe(403);

        var publishDraft = await merchant.Client.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProblemDetails>(new(draft.Id));
        publishDraft.Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        publishDraft.Result.Status.ShouldBe(403);

        var unpublish = await merchant.Client
            .POSTAsync<UnpublishProductEndpoint, ChangeProductStatusRequest, ProblemDetails>(new(published.Id));
        unpublish.Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        unpublish.Result.Status.ShouldBe(403);
    }

    [Fact]
    public async Task Merchant_list_requires_operator_role()
    {
        (await App.ClientFor(TestActor.Buyer()).GETAsync<ListMerchantsEndpoint, ProblemDetails>())
            .Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await App.Client.GETAsync<ListMerchantsEndpoint, ProblemDetails>())
            .Response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
