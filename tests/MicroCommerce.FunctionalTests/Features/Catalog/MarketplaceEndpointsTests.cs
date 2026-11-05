using System.Net;
using System.Net.Http.Json;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.Domain.Catalog;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.FunctionalTests.Features.Catalog;

/// <summary>Anyone browses Published Products across Merchants (User Story 3, FR-024 to FR-032).</summary>
public class MarketplaceEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>A word no other test uses, so a search finds only this test's Products.</summary>
    private static string Token() => $"tok{Guid.NewGuid():N}"[..20];

    [Fact]
    public async Task The_list_spans_merchants_and_shows_cards_without_stock_or_addresses()
    {
        var token = Token();
        var (first, firstShop, _) = await App.NewMerchantAsync();
        var (second, secondShop, _) = await App.NewMerchantAsync();
        var a = await first.PublishAsync(await first.CreateSimpleProductAsync($"{token} mug", price: 100_000));
        var b = await second.PublishAsync(await second.CreateSimpleProductAsync($"{token} cup", price: 50_000));

        var response = await App.Client.GetAsync($"/api/marketplace/products?query={token}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(Ct);
        json.ShouldNotContain("\"stock\"");
        json.ShouldNotContain("totalStock");
        json.ShouldNotContain("weightGrams");
        json.ShouldNotContain("pickupAddress");
        var page = (await response.Content.ReadFromJsonAsync<PageDto<ProductCardDto>>(Ct))!;
        page.Items.Select(c => c.Id).Order().ShouldBe(new[] { a.Id, b.Id }.Order());
        var card = page.Items.Single(c => c.Id == a.Id);
        (card.MinPrice, card.MaxPrice, card.Available).ShouldBe((100_000L, 100_000L, true));
        card.Shop.ShouldBe(new ShopRefDto(firstShop.MerchantId, firstShop.ShopName));
        card.CoverImageUrl.ShouldBeNull();
        page.Items.Single(c => c.Id == b.Id).Shop.ShopName.ShouldBe(secondShop.ShopName);
    }

    [Fact]
    public async Task Pages_are_newest_first_and_clamped()
    {
        var (merchant, shop, _) = await App.NewMerchantAsync();
        var created = new List<Guid>();
        for (var i = 0; i < 3; i++) created.Add((await merchant.PublishAsync(await merchant.CreateSimpleProductAsync())).Id);

        var firstPage = await App.Client.BrowseAsync($"shopId={shop.MerchantId}&pageSize=2");
        var secondPage = await App.Client.BrowseAsync($"shopId={shop.MerchantId}&pageSize=2&page=2");
        var pastTheEnd = await App.Client.BrowseAsync($"shopId={shop.MerchantId}&pageSize=2&page=9");
        var clamped = await App.Client.BrowseAsync($"shopId={shop.MerchantId}&pageSize=500");

        firstPage.Items.Select(c => c.Id).ShouldBe([created[2], created[1]]);
        secondPage.Items.Select(c => c.Id).ShouldBe([created[0]]);
        firstPage.TotalCount.ShouldBe(3);
        pastTheEnd.Items.ShouldBeEmpty();
        pastTheEnd.TotalCount.ShouldBe(3);
        clamped.PageSize.ShouldBe(100);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    public async Task Invalid_paging_is_refused(string query) =>
        (await App.Client.GetAsync($"/api/marketplace/products?{query}", Ct)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    [Fact]
    public async Task An_overlong_query_is_refused()
    {
        var response = await App.Client.GetAsync($"/api/marketplace/products?query={new string('a', 201)}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.ProblemAsync()).Errors!.Keys.ShouldBe(["query"]);
    }

    [Fact]
    public async Task Drafts_are_hidden_until_published_and_hidden_again_once_unpublished()
    {
        var token = Token();
        var (merchant, shop, _) = await App.NewMerchantAsync();
        var draft = await merchant.CreateSimpleProductAsync($"{token} vase");

        (await App.Client.BrowseAsync($"query={token}")).Items.ShouldBeEmpty();
        (await App.Client.BrowseAsync($"shopId={shop.MerchantId}")).Items.ShouldBeEmpty();
        (await App.Client.GetAsync($"/api/marketplace/products/{draft.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var published = await merchant.PublishAsync(draft);
        (await App.Client.BrowseAsync($"query={token}")).Items.ShouldHaveSingleItem().Id.ShouldBe(draft.Id);
        (await App.Client.GetAsync($"/api/marketplace/products/{draft.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await merchant.PostAsJsonAsync($"/api/merchant/products/{draft.Id}/unpublish", new { version = published.Version }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
        (await App.Client.BrowseAsync($"query={token}")).Items.ShouldBeEmpty();
        (await App.Client.GetAsync($"/api/marketplace/products/{draft.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Search_matches_word_variations_in_name_or_description_and_needs_every_word()
    {
        var token = Token();
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var mug = await merchant.PublishAsync(await merchant.CreateSimpleProductAsync($"{token} Ceramic coffee mug"));
        var described = await merchant.PublishAsync(await merchant.CreateProductAsync(
            ProductHelpers.ProductRequest($"{token} Teapot", kitchen, description: "Pours like a dream, matches our mugs")));

        (await App.Client.BrowseAsync($"query={token} mugs")).Items.Select(c => c.Id).Order().ShouldBe(new[] { mug.Id, described.Id }.Order());
        (await App.Client.BrowseAsync($"query={token} coffee")).Items.ShouldHaveSingleItem().Id.ShouldBe(mug.Id);
        (await App.Client.BrowseAsync($"query={token} coffee teapot")).Items.ShouldBeEmpty();
        (await App.Client.BrowseAsync($"query={Token()}")).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task The_category_filter_includes_descendants_and_combines_with_search()
    {
        var token = Token();
        var (top, leaf) = await NewBranchAsync();
        var (merchant, _, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var inLeaf = await merchant.PublishAsync(await merchant.CreateProductAsync(ProductHelpers.ProductRequest($"{token} lamp", leaf)));
        await merchant.PublishAsync(await merchant.CreateProductAsync(ProductHelpers.ProductRequest($"{token} lamp", kitchen)));

        (await App.Client.BrowseAsync($"categoryId={top}")).Items.ShouldHaveSingleItem().Id.ShouldBe(inLeaf.Id);
        (await App.Client.BrowseAsync($"categoryId={leaf}&query={token}")).Items.ShouldHaveSingleItem().Id.ShouldBe(inLeaf.Id);
        (await App.Client.BrowseAsync($"query={token}")).TotalCount.ShouldBe(2);
        var unknown = await App.Client.BrowseAsync($"categoryId={Guid.NewGuid()}");
        unknown.Items.ShouldBeEmpty();
        unknown.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task The_product_page_shows_availability_but_never_stock_or_weight()
    {
        var (merchant, shop, _) = await App.NewMerchantAsync();
        var kitchen = await CatalogTestData.SeededLeafCategoryAsync(App.Client);
        var product = await merchant.PublishAsync(await merchant.CreateProductAsync(ProductHelpers.ProductRequest(
            CatalogTestData.Unique("Mug"), kitchen,
            options: [new { name = "Size", values = new[] { "S", "L" } }],
            variants: [ProductHelpers.VariantRequest(stock: 7, optionValues: "S"), ProductHelpers.VariantRequest(stock: 0, optionValues: "L")])));

        var response = await App.Client.GetAsync($"/api/marketplace/products/{product.Id}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(Ct);
        json.ShouldNotContain("\"stock\"");
        json.ShouldNotContain("weightGrams");
        var detail = (await response.Content.ReadFromJsonAsync<ProductDetailDto>(Ct))!;
        detail.Category.ShouldBe(new CategoryRefDto(kitchen, "Kitchen"));
        detail.Options.ShouldHaveSingleItem().Values.ShouldBe(["S", "L"]);
        detail.Variants.Select(v => (v.OptionValues.Single(), v.Availability)).ShouldBe([("S", 7), ("L", 0)]);
        detail.Images.ShouldBeEmpty();
        detail.Shop.ShouldBe(new ShopRefDto(shop.MerchantId, shop.ShopName));
    }

    [Fact]
    public async Task A_product_whose_variants_are_all_out_of_stock_is_not_available()
    {
        var token = Token();
        var (merchant, _, _) = await App.NewMerchantAsync();
        await merchant.PublishAsync(await merchant.CreateSimpleProductAsync($"{token} jug", stock: 0));

        (await App.Client.BrowseAsync($"query={token}")).Items.ShouldHaveSingleItem().Available.ShouldBeFalse();
    }

    [Fact]
    public async Task An_unknown_product_is_not_found() =>
        (await App.Client.GetAsync($"/api/marketplace/products/{Guid.NewGuid()}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

    [Fact]
    public async Task The_shop_page_shows_the_shop_but_not_its_pickup_address()
    {
        var (_, shop, _) = await App.NewMerchantAsync();

        var response = await App.Client.GetAsync($"/api/marketplace/shops/{shop.MerchantId}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(Ct)).ShouldNotContain("pickupAddress", Case.Insensitive);
        (await response.Content.ReadFromJsonAsync<ShopPageDto>(Ct)).ShouldBe(new ShopPageDto(shop.MerchantId, shop.ShopName, shop.Description));
        (await App.Client.GetAsync($"/api/marketplace/shops/{Guid.NewGuid()}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_shop_rename_shows_on_its_cards_at_once()
    {
        var token = Token();
        var (merchant, shop, _) = await App.NewMerchantAsync();
        await merchant.PublishAsync(await merchant.CreateSimpleProductAsync($"{token} bowl"));
        var newName = CatalogTestData.UniqueShopName();

        (await merchant.PutAsJsonAsync("/api/merchant/shop",
            new { shopName = newName, description = shop.Description, pickupAddress = shop.PickupAddress, version = shop.Version }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.OK);

        (await App.Client.BrowseAsync($"query={token}")).Items.Single().Shop.ShopName.ShouldBe(newName);
    }

    [Fact]
    public async Task Searching_with_the_other_grammatical_number_finds_at_least_18_of_20_products()
    {
        (string Named, string Searched)[] pairs =
        [
            ("mug", "mugs"), ("dresses", "dress"), ("shirt", "shirts"), ("boxes", "box"), ("bottle", "bottles"),
            ("glasses", "glass"), ("knife", "knives"), ("lamps", "lamp"), ("pillow", "pillows"), ("cups", "cup"),
            ("bag", "bags"), ("watches", "watch"), ("shoe", "shoes"), ("socks", "sock"), ("phone", "phones"),
            ("laptops", "laptop"), ("plate", "plates"), ("bowls", "bowl"), ("towel", "towels"), ("brushes", "brush"),
        ];
        var token = Token();
        var (merchant, _, _) = await App.NewMerchantAsync();
        foreach (var (named, _) in pairs) await merchant.PublishAsync(await merchant.CreateSimpleProductAsync($"{token} {named}"));

        var found = 0;
        foreach (var (_, searched) in pairs)
        {
            if ((await App.Client.BrowseAsync($"query={token} {searched}")).Items.Count == 1) found++;
        }

        found.ShouldBeGreaterThanOrEqualTo(18);
    }

    /// <summary>A fresh two-level branch of the Category tree: (top, leaf under it).</summary>
    private async Task<(Guid Top, Guid Leaf)> NewBranchAsync()
    {
        await using var scope = App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var top = Category.CreateTopLevel(CategoryId.New(), CatalogTestData.Unique("Branch"));
        var leaf = Category.CreateChild(CategoryId.New(), "Leaf", top.Id);
        db.Categories.AddRange(top, leaf);
        await db.SaveChangesAsync(Ct);
        return (top.Id.Value, leaf.Id.Value);
    }
}
