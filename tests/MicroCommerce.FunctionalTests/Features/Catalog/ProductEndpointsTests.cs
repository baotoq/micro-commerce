using System.Net;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

public class ProductEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static readonly PickupAddress Address = new("12 Le Loi", "Ben Nghe", "District 1", "Ho Chi Minh City");

    private readonly HttpClient _operator = App.ClientFor(TestActor.PlatformOperator());

    // The database is shared across the test run, so every test makes its own Merchants, Categories and Products.
    private static string Unique(string prefix) => $"{prefix} {Guid.NewGuid():N}";

    /// <summary>A client for a fresh Account that owns a fresh Merchant.</summary>
    private async Task<HttpClient> NewMerchantAsync()
    {
        var client = App.ClientFor(TestActor.Buyer());
        var (rsp, _) = await client.POSTAsync<OpenMerchantEndpoint, OpenMerchantRequest, MerchantResponse>(
            new(Unique("Shop"), Address));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        return client;
    }

    private async Task<CategoryResponse> NewCategoryAsync(CategoryId? parentId = null)
    {
        var (rsp, category) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(
            new(Unique("Category"), parentId));
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        return category;
    }

    private static CreateProductRequest NewProduct(CategoryId categoryId, long price = 120_000, int stock = 5, int weight = 350) =>
        new(Unique("Mug"), "A sturdy stoneware mug.", categoryId, price, stock, weight);

    private static async Task<ProductResponse> CreateAsync(HttpClient merchant, CreateProductRequest request)
    {
        var (rsp, product) = await merchant.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(request);
        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        return product;
    }

    private static UpdateProductRequest UpdateOf(ProductResponse product, CategoryId? categoryId = null) =>
        new(product.Id, Unique("Mug"), "Now in matte black.", categoryId ?? product.CategoryId, 150_000, 2, 400);

    [Fact]
    public async Task Merchant_creates_a_draft_product_with_a_single_default_variant()
    {
        var merchant = await NewMerchantAsync();
        var category = await NewCategoryAsync();
        var request = NewProduct(category.Id) with { Name = $"  {Unique("Mug")}  ", Description = "  Stoneware.  " };

        var (rsp, product) = await merchant.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(request);

        rsp.StatusCode.ShouldBe(HttpStatusCode.Created);
        rsp.Headers.Location!.ToString().ShouldBe($"/merchant/products/{product.Id}");
        product.Name.ShouldBe(request.Name.Trim());
        product.Description.ShouldBe("Stoneware.");
        product.CategoryId.ShouldBe(category.Id);
        product.Status.ShouldBe(ProductStatus.Draft);
        var variant = product.Variants.ShouldHaveSingleItem();
        variant.Price.ShouldBe(Money.From(120_000));
        variant.Stock.ShouldBe(5);
        variant.WeightGrams.ShouldBe(350);

        var (getRsp, read) = await merchant.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(new(product.Id));
        getRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        read.ShouldBeEquivalentTo(product);
    }

    [Fact]
    public async Task Blank_description_is_stored_as_none()
    {
        var merchant = await NewMerchantAsync();

        var product = await CreateAsync(merchant, NewProduct((await NewCategoryAsync()).Id) with { Description = "  " });

        product.Description.ShouldBeNull();
    }

    [Fact]
    public async Task Creating_with_invalid_details_is_a_bad_request()
    {
        var merchant = await NewMerchantAsync();

        var (rsp, problem) = await merchant.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProblemDetails>(
            new(" ", null, null, 0, -1, 0));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["name", "categoryId", "price", "stock", "weightGrams"], ignoreOrder: true);
    }

    [Fact]
    public async Task Creating_in_an_unknown_category_is_a_bad_request()
    {
        var merchant = await NewMerchantAsync();

        var (rsp, problem) = await merchant.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProblemDetails>(
            NewProduct(CategoryId.New()));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["categoryId"]);
    }

    [Fact]
    public async Task Creating_in_a_category_with_children_is_a_bad_request()
    {
        var merchant = await NewMerchantAsync();
        var parent = await NewCategoryAsync();
        await NewCategoryAsync(parent.Id);

        var (rsp, problem) = await merchant.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProblemDetails>(
            NewProduct(parent.Id));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["categoryId"]);
        var (_, products) = await merchant.GETAsync<ListProductsEndpoint, IReadOnlyList<ProductSummaryResponse>>();
        products.ShouldBeEmpty();
    }

    [Fact]
    public async Task Merchant_publishes_and_unpublishes_a_product()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreateAsync(merchant, NewProduct((await NewCategoryAsync()).Id));

        var (publishRsp, published) = await merchant
            .POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        publishRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        published.Status.ShouldBe(ProductStatus.Published);
        (await StatusAsync(merchant, product.Id)).ShouldBe(ProductStatus.Published);

        var (_, publishedAgain) = await merchant
            .POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        publishedAgain.Status.ShouldBe(ProductStatus.Published);

        var (unpublishRsp, unpublished) = await merchant
            .POSTAsync<UnpublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        unpublishRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        unpublished.Status.ShouldBe(ProductStatus.Draft);
        (await StatusAsync(merchant, product.Id)).ShouldBe(ProductStatus.Draft);
    }

    [Fact]
    public async Task Editing_replaces_the_details_and_keeps_the_status_and_variant()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreateAsync(merchant, NewProduct((await NewCategoryAsync()).Id));
        await merchant.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(product.Id));
        var otherCategory = await NewCategoryAsync();
        var update = UpdateOf(product, otherCategory.Id);

        var (rsp, updated) = await merchant.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProductResponse>(update);

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldBeEquivalentTo(product with
        {
            Name = update.Name,
            Description = update.Description,
            CategoryId = otherCategory.Id,
            Status = ProductStatus.Published,
            Variants = new List<VariantResponse> { new(product.Variants.Single().Id, Money.From(150_000), 2, 400) },
        });
        var (_, read) = await merchant.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(new(product.Id));
        read.ShouldBeEquivalentTo(updated);
    }

    [Fact]
    public async Task Editing_with_invalid_details_is_a_bad_request()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreateAsync(merchant, NewProduct((await NewCategoryAsync()).Id));

        var (rsp, problem) = await merchant.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProblemDetails>(
            UpdateOf(product) with { Name = "", Price = -5 });

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["name", "price"], ignoreOrder: true);
    }

    [Fact]
    public async Task Moving_a_product_to_a_category_with_children_is_a_bad_request()
    {
        var merchant = await NewMerchantAsync();
        var product = await CreateAsync(merchant, NewProduct((await NewCategoryAsync()).Id));
        var parent = await NewCategoryAsync();
        await NewCategoryAsync(parent.Id);

        var (rsp, problem) = await merchant.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProblemDetails>(
            UpdateOf(product, parent.Id));

        rsp.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        problem.Errors.Select(e => e.Name).ShouldBe(["categoryId"]);
        var (_, read) = await merchant.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(new(product.Id));
        read.ShouldBeEquivalentTo(product);
    }

    [Fact]
    public async Task List_shows_every_own_product_with_price_range_and_total_stock()
    {
        var merchant = await NewMerchantAsync();
        var category = await NewCategoryAsync();
        var draft = await CreateAsync(merchant, NewProduct(category.Id, price: 50_000, stock: 0));
        var published = await CreateAsync(merchant, NewProduct(category.Id, price: 99_000, stock: 7));
        await merchant.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProductResponse>(new(published.Id));
        await CreateAsync(await NewMerchantAsync(), NewProduct(category.Id));

        var (rsp, products) = await merchant.GETAsync<ListProductsEndpoint, IReadOnlyList<ProductSummaryResponse>>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        products.ShouldBe([
            new(published.Id, published.Name, category.Id, ProductStatus.Published, 1, Money.From(99_000), Money.From(99_000), 7, published.CreatedAt),
            new(draft.Id, draft.Name, category.Id, ProductStatus.Draft, 1, Money.From(50_000), Money.From(50_000), 0, draft.CreatedAt),
        ]);
    }

    [Fact]
    public async Task Another_merchants_product_is_not_found_for_reads_and_writes()
    {
        var owner = await NewMerchantAsync();
        var product = await CreateAsync(owner, NewProduct((await NewCategoryAsync()).Id));
        var other = await NewMerchantAsync();

        var statuses = await StatusesAsync(other, product);

        statuses.ShouldAllBe(s => s == HttpStatusCode.NotFound);
        var (_, read) = await owner.GETAsync<GetProductEndpoint, GetProductRequest, ProductResponse>(new(product.Id));
        read.ShouldBeEquivalentTo(product);
        var (_, otherList) = await other.GETAsync<ListProductsEndpoint, IReadOnlyList<ProductSummaryResponse>>();
        otherList.ShouldBeEmpty();
    }

    [Fact]
    public async Task Unknown_product_is_not_found()
    {
        var merchant = await NewMerchantAsync();
        var category = await NewCategoryAsync();

        var statuses = await StatusesAsync(merchant, new ProductResponse(
            ProductId.New(), "Mug", null, category.Id, ProductStatus.Draft, [], DateTimeOffset.UtcNow));

        statuses.ShouldAllBe(s => s == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Accounts_without_a_merchant_are_refused()
    {
        var product = await CreateAsync(await NewMerchantAsync(), NewProduct((await NewCategoryAsync()).Id));
        var buyer = App.ClientFor(TestActor.Buyer());

        var statuses = await StatusesAsync(buyer, product);

        statuses.ShouldAllBe(s => s == HttpStatusCode.Forbidden);
        (await buyer.GETAsync<ListProductsEndpoint, ProblemDetails>()).Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await buyer.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProblemDetails>(NewProduct(product.CategoryId)))
            .Response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_requests_are_unauthorized()
    {
        var product = await CreateAsync(await NewMerchantAsync(), NewProduct((await NewCategoryAsync()).Id));

        var statuses = await StatusesAsync(App.Client, product);

        statuses.ShouldAllBe(s => s == HttpStatusCode.Unauthorized);
        (await App.Client.GETAsync<ListProductsEndpoint, ProblemDetails>()).Response.StatusCode
            .ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Category_holding_products_cannot_gain_children()
    {
        var category = await NewCategoryAsync();
        await CreateAsync(await NewMerchantAsync(), NewProduct(category.Id));

        var (rsp, problem) = await _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, ProblemDetails>(
            new(Unique("Child"), category.Id));

        rsp.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.Errors.ShouldHaveSingleItem().Name.ShouldBe("parentId");
    }

    [Fact]
    public async Task Category_holding_products_cannot_be_deleted()
    {
        var category = await NewCategoryAsync();
        await CreateAsync(await NewMerchantAsync(), NewProduct(category.Id));

        var (rsp, problem) = await _operator.DELETEAsync<DeleteCategoryEndpoint, DeleteCategoryRequest, ProblemDetails>(
            new(category.Id));

        rsp.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        problem.Errors.ShouldHaveSingleItem().Reason.ShouldBe("This Category holds Products.");
        var (getRsp, _) = await _operator.GETAsync<GetCategoryEndpoint, GetCategoryRequest, CategoryResponse>(new(category.Id));
        getRsp.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Concurrent_product_and_child_category_never_leave_a_product_in_a_non_leaf()
    {
        var merchant = await NewMerchantAsync();

        for (var i = 0; i < 5; i++)
        {
            var category = await NewCategoryAsync();
            var create = merchant.POSTAsync<CreateProductEndpoint, CreateProductRequest, ProductResponse>(NewProduct(category.Id));
            var addChild = _operator.POSTAsync<CreateCategoryEndpoint, CreateCategoryRequest, CategoryResponse>(
                new(Unique("Child"), category.Id));

            var (created, childAdded) = ((await create).Response.IsSuccessStatusCode, (await addChild).Response.IsSuccessStatusCode);

            (created && childAdded).ShouldBeFalse();
            (created || childAdded).ShouldBeTrue();
        }
    }

    private static async Task<ProductStatus> StatusAsync(HttpClient merchant, ProductId id)
    {
        var (_, products) = await merchant.GETAsync<ListProductsEndpoint, IReadOnlyList<ProductSummaryResponse>>();
        return products.Single(p => p.Id == id).Status;
    }

    /// <summary>Tries every Merchant Centre endpoint that names <paramref name="product"/>.</summary>
    private static async Task<HttpStatusCode[]> StatusesAsync(HttpClient client, ProductResponse product) =>
    [
        (await client.GETAsync<GetProductEndpoint, GetProductRequest, ProblemDetails>(new(product.Id))).Response.StatusCode,
        (await client.PUTAsync<UpdateProductEndpoint, UpdateProductRequest, ProblemDetails>(UpdateOf(product))).Response.StatusCode,
        (await client.POSTAsync<PublishProductEndpoint, ChangeProductStatusRequest, ProblemDetails>(new(product.Id))).Response.StatusCode,
        (await client.POSTAsync<UnpublishProductEndpoint, ChangeProductStatusRequest, ProblemDetails>(new(product.Id))).Response.StatusCode,
    ];
}
