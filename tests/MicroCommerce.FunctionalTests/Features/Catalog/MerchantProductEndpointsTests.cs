using System.Net;
using System.Net.Http.Json;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

/// <summary>A Merchant lists Products in the Merchant Centre (User Story 2, FR-007 to FR-019).</summary>
public class MerchantProductEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Creating_a_product_gives_a_draft_with_one_default_variant()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var name = CatalogTestData.Unique("Ceramic coffee mug");

        var response = await merchant.PostAsJsonAsync("/api/merchant/products",
            ProductHelpers.ProductRequest(name, kitchen, variants: [ProductHelpers.VariantRequest(price: 120_000, stock: 10, weightGrams: 400)]), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var product = (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!;
        response.Headers.Location!.ToString().ShouldBe($"/api/merchant/products/{product.Id}");
        product.Name.ShouldBe(name);
        product.CategoryId.ShouldBe(kitchen);
        product.Status.ShouldBe("Draft");
        product.Version.ShouldBe(1);
        product.Options.ShouldBeEmpty();
        product.Images.ShouldBeEmpty();
        var variant = product.Variants.ShouldHaveSingleItem();
        variant.OptionValues.ShouldBeEmpty();
        (variant.Price, variant.Stock, variant.WeightGrams).ShouldBe((120_000L, 10, 400));

        (await merchant.GetOwnProductAsync(product.Id)).ShouldBeEquivalentTo(product);
    }

    [Fact]
    public async Task Invalid_input_names_each_invalid_field()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);

        var response = await merchant.PostAsJsonAsync("/api/merchant/products",
            ProductHelpers.ProductRequest("", kitchen, variants: [ProductHelpers.VariantRequest(price: 0, stock: -1, weightGrams: 0)]), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.ProblemAsync()).Errors!.Keys.Order()
            .ShouldBe(["name", "variants[0].price", "variants[0].stock", "variants[0].weightGrams"]);
    }

    [Fact]
    public async Task A_variant_without_option_values_is_a_400_on_the_field()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);

        var response = await merchant.PostAsJsonAsync("/api/merchant/products", new
        {
            name = "Mug",
            categoryId = kitchen,
            options = Array.Empty<object>(),
            variants = new[] { new { price = 120_000, stock = 1, weightGrams = 400 } },
        }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.ProblemAsync()).Errors!.Keys.ShouldBe(["variants[0].optionValues"]);
    }

    [Fact]
    public async Task An_unknown_or_non_leaf_category_is_refused_on_categoryId()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var home = await CatalogTestData.SeededHomeCategoryAsync(App.Client);

        foreach (var categoryId in new[] { Guid.NewGuid(), home })
        {
            var response = await merchant.PostAsJsonAsync("/api/merchant/products", ProductHelpers.ProductRequest("Mug", categoryId), Ct);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            (await response.ProblemAsync()).Errors!.Keys.ShouldBe(["categoryId"]);
        }
    }

    [Fact]
    public async Task Editing_keeps_the_status_and_the_variant_and_moves_the_version_on()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.PublishAsync(await merchant.CreateSimpleProductAsync());
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);

        var response = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}", new
        {
            name = "Bigger mug",
            description = (string?)null,
            categoryId = kitchen,
            options = Array.Empty<object>(),
            variants = new[] { ProductHelpers.VariantRequest(price: 150_000, stock: 3) },
            version = product.Version,
        }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var edited = (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!;
        edited.Name.ShouldBe("Bigger mug");
        edited.Description.ShouldBeNull();
        edited.Status.ShouldBe("Published");
        edited.Version.ShouldBe(product.Version + 1);
        edited.Variants.Single().Id.ShouldBe(product.Variants.Single().Id);
        (edited.Variants.Single().Price, edited.Variants.Single().Stock).ShouldBe((150_000L, 3));
    }

    [Fact]
    public async Task Moving_to_a_non_leaf_category_is_refused()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();
        var home = await CatalogTestData.SeededHomeCategoryAsync(App.Client);

        var response = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}",
            UpdateBody(product, categoryId: home), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.ProblemAsync()).Errors!.Keys.ShouldBe(["categoryId"]);
    }

    [Fact]
    public async Task Publishing_and_unpublishing_move_the_status()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();

        var published = await merchant.PublishAsync(product);
        published.Status.ShouldBe("Published");

        var response = await merchant.PostAsJsonAsync($"/api/merchant/products/{product.Id}/unpublish", new { version = published.Version }, Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!.Status.ShouldBe("Draft");
    }

    [Fact]
    public async Task A_stale_version_is_refused()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();
        await merchant.PublishAsync(product);

        var edit = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}", UpdateBody(product), Ct);
        var publish = await merchant.PostAsJsonAsync($"/api/merchant/products/{product.Id}/unpublish", new { version = product.Version }, Ct);

        foreach (var response in new[] { edit, publish })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            (await response.ProblemAsync()).Type.ShouldBe("product-version-conflict");
        }
    }

    [Fact]
    public async Task Of_two_parallel_edits_from_the_same_version_exactly_one_wins()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var product = await merchant.CreateSimpleProductAsync();

        var responses = await Task.WhenAll(Enumerable.Range(0, 2).Select(i =>
            merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}", UpdateBody(product, name: $"Edit {i}"), Ct)));

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(1);
    }

    [Fact]
    public async Task The_list_shows_only_own_products_newest_first_with_price_range_and_total_stock()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var (other, _, _) = await App.NewMerchantAsync();
        await other.CreateSimpleProductAsync();
        var first = await merchant.CreateSimpleProductAsync(price: 100_000, stock: 4);
        var second = await merchant.PublishAsync(await merchant.CreateSimpleProductAsync(price: 250_000, stock: 6));

        var page = (await merchant.GetFromJsonAsync<PageDto<MerchantProductSummaryDto>>("/api/merchant/products", Ct))!;

        page.TotalCount.ShouldBe(2);
        page.Items.Select(p => p.Id).ShouldBe([second.Id, first.Id]);
        var summary = page.Items[0];
        summary.Status.ShouldBe("Published");
        summary.VariantCount.ShouldBe(1);
        (summary.MinPrice, summary.MaxPrice, summary.TotalStock).ShouldBe((250_000L, 250_000L, 6L));
        summary.CoverImageUrl.ShouldBeNull();
    }

    [Fact]
    public async Task Another_merchants_product_does_not_exist_for_reads_or_writes()
    {
        var (owner, _, _) = await App.NewMerchantAsync();
        var (stranger, _, _) = await App.NewMerchantAsync();
        var product = await owner.CreateSimpleProductAsync();

        (await stranger.GetAsync($"/api/merchant/products/{product.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await stranger.PutAsJsonAsync($"/api/merchant/products/{product.Id}", UpdateBody(product, name: "Hijacked"), Ct))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await stranger.PostAsJsonAsync($"/api/merchant/products/{product.Id}/publish", new { version = product.Version }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await stranger.PostAsJsonAsync($"/api/merchant/products/{product.Id}/unpublish", new { version = product.Version }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await owner.GetOwnProductAsync(product.Id)).ShouldBeEquivalentTo(product);
    }

    [Fact]
    public async Task An_unknown_product_is_not_found()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();

        (await merchant.GetAsync($"/api/merchant/products/{Guid.NewGuid()}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Every_route_needs_a_merchant_and_a_sign_in()
    {
        var withoutMerchant = App.ClientFor(TestActor.Buyer());
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var id = Guid.NewGuid();
        var body = ProductHelpers.ProductRequest("Mug", kitchen);

        foreach (var client in new[] { withoutMerchant, App.Client })
        {
            var expected = client == App.Client ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden;
            (await client.GetAsync("/api/merchant/products", Ct)).StatusCode.ShouldBe(expected);
            (await client.PostAsJsonAsync("/api/merchant/products", body, Ct)).StatusCode.ShouldBe(expected);
            (await client.GetAsync($"/api/merchant/products/{id}", Ct)).StatusCode.ShouldBe(expected);
            (await client.PutAsJsonAsync($"/api/merchant/products/{id}", body, Ct)).StatusCode.ShouldBe(expected);
            (await client.PostAsJsonAsync($"/api/merchant/products/{id}/publish", new { version = 1 }, Ct)).StatusCode.ShouldBe(expected);
            (await client.PostAsJsonAsync($"/api/merchant/products/{id}/unpublish", new { version = 1 }, Ct)).StatusCode.ShouldBe(expected);
        }
    }

    internal static object UpdateBody(MerchantProductDto product, string? name = null, Guid? categoryId = null) => new
    {
        name = name ?? product.Name,
        description = product.Description,
        categoryId = categoryId ?? product.CategoryId,
        options = product.Options,
        variants = product.Variants.Select(v => new { v.OptionValues, v.Price, v.Stock, v.WeightGrams }).ToArray(),
        version = product.Version,
    };
}

/// <summary>Up to two Options, whose combinations are the Variants (User Story 4, FR-010 to FR-013).</summary>
public class ProductOptionsEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static readonly object Colour = new { name = "Colour", values = new[] { "Red", "Blue" } };
    private static readonly object Size = new { name = "Size", values = new[] { "S", "M", "L" } };

    private static object[] Grid(string[] colours, string[] sizes, int stock = 5) =>
        colours.SelectMany(c => sizes.Select(s => ProductHelpers.VariantRequest(price: s == "L" ? 150_000 : 120_000, stock: stock, optionValues: [c, s]))).ToArray();

    [Fact]
    public async Task Two_options_give_six_variants_in_option_order_however_they_are_posted()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);

        var product = await merchant.CreateProductAsync(ProductHelpers.ProductRequest("Tee", kitchen,
            options: [Colour, Size], variants: Grid(["Blue", "Red"], ["L", "S", "M"])));

        product.Variants.Select(v => string.Join('/', v.OptionValues)).ShouldBe(["Red/S", "Red/M", "Red/L", "Blue/S", "Blue/M", "Blue/L"]);
        product.Options.Select(o => o.Name).ShouldBe(["Colour", "Size"]);
    }

    [Fact]
    public async Task Variants_that_are_not_exactly_the_combinations_are_refused()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);

        var response = await merchant.PostAsJsonAsync("/api/merchant/products", ProductHelpers.ProductRequest("Tee", kitchen,
            options: [Colour, Size], variants: Grid(["Red"], ["S", "M", "L"])), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.ProblemAsync();
        problem.Errors!.Keys.ShouldBe(["variants"]);
        problem.Errors["variants"].ShouldContain(m => m.Contains("Blue / S"));
    }

    [Fact]
    public async Task Invalid_options_are_refused()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var fit = new { name = "Fit", values = new[] { "Slim" } };

        var response = await merchant.PostAsJsonAsync("/api/merchant/products", ProductHelpers.ProductRequest("Tee", kitchen,
            options: [Colour, Size, fit], variants: []), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.ProblemAsync()).Errors!.Keys.ShouldContain("options");
    }

    [Fact]
    public async Task Removing_a_value_keeps_the_surviving_variant_ids()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var product = await merchant.CreateProductAsync(ProductHelpers.ProductRequest("Tee", kitchen, options: [Colour, Size], variants: Grid(["Red", "Blue"], ["S", "M", "L"])));
        var before = product.Variants.ToDictionary(v => string.Join('/', v.OptionValues), v => v.Id);

        var response = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}", new
        {
            name = "Tee",
            description = (string?)null,
            categoryId = kitchen,
            options = new[] { Colour, new { name = "Size", values = new[] { "S", "M" } } },
            variants = Grid(["Red", "Blue"], ["S", "M"]),
            version = product.Version,
        }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var edited = (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!;
        edited.Variants.Count.ShouldBe(4);
        edited.Variants.ShouldAllBe(v => before[string.Join('/', v.OptionValues)] == v.Id);
    }

    [Fact]
    public async Task Changing_only_prices_and_stock_keeps_every_variant_id()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var product = await merchant.CreateProductAsync(ProductHelpers.ProductRequest("Tee", kitchen, options: [Colour, Size], variants: Grid(["Red", "Blue"], ["S", "M", "L"])));

        var response = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}", new
        {
            name = "Tee", description = (string?)null, categoryId = kitchen, options = new[] { Colour, Size },
            variants = Grid(["Red", "Blue"], ["S", "M", "L"], stock: 99), version = product.Version,
        }, Ct);

        var edited = (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!;
        edited.Variants.Select(v => v.Id).ShouldBe(product.Variants.Select(v => v.Id));
        edited.Variants.ShouldAllBe(v => v.Stock == 99);
    }

    [Fact]
    public async Task Removing_every_option_leaves_the_default_variant()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var product = await merchant.CreateProductAsync(ProductHelpers.ProductRequest("Tee", kitchen, options: [Colour], variants:
            [ProductHelpers.VariantRequest(optionValues: "Red"), ProductHelpers.VariantRequest(optionValues: "Blue")]));

        var response = await merchant.PutAsJsonAsync($"/api/merchant/products/{product.Id}", new
        {
            name = "Tee", description = (string?)null, categoryId = kitchen, options = Array.Empty<object>(),
            variants = new[] { ProductHelpers.VariantRequest() }, version = product.Version,
        }, Ct);

        (await response.Content.ReadFromJsonAsync<MerchantProductDto>(Ct))!.Variants.ShouldHaveSingleItem().OptionValues.ShouldBeEmpty();
    }

    [Fact]
    public async Task The_list_and_the_marketplace_cover_every_variant()
    {
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var product = await merchant.PublishAsync(await merchant.CreateProductAsync(ProductHelpers.ProductRequest("Tee", kitchen,
            options: [Colour, Size], variants: Grid(["Red", "Blue"], ["S", "M", "L"]))));

        var summary = (await merchant.GetFromJsonAsync<PageDto<MerchantProductSummaryDto>>("/api/merchant/products", Ct))!.Items.Single();
        (summary.VariantCount, summary.MinPrice, summary.MaxPrice, summary.TotalStock).ShouldBe((6, 120_000L, 150_000L, 30L));

        var detail = (await App.Client.GetFromJsonAsync<ProductDetailDto>($"/api/marketplace/products/{product.Id}", Ct))!;
        detail.Variants.Select(v => string.Join('/', v.OptionValues)).ShouldBe(["Red/S", "Red/M", "Red/L", "Blue/S", "Blue/M", "Blue/L"]);
    }
}
