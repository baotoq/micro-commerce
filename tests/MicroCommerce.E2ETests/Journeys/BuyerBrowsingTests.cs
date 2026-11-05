using static MicroCommerce.E2ETests.CatalogArrange;

namespace MicroCommerce.E2ETests.Journeys;

/// <summary>
/// A Buyer finds and opens Products across Merchants (User Story 2). Data is arranged through the
/// API; the browser only browses, signed out, as a visitor would.
/// </summary>
[Trait("Journey", "Buyer")]
public class BuyerBrowsingTests(MarketplaceFixture fixture) : BrowserTest(fixture)
{
    // US2 #1
    [Fact]
    public async Task A_visitor_sees_published_products_from_two_merchants()
    {
        var (firstToken, firstShop) = await NewMerchantAsync();
        var (secondToken, secondShop) = await NewMerchantAsync();
        var first = await PublishedProductAsync(firstToken, TestRun.Unique("Mug"));
        var second = await PublishedProductAsync(secondToken, TestRun.Unique("Mug"));

        // No search or filter: the scenario is the home listing. It shows the newest 20 first, and the
        // whole run creates fewer than 20 Products (about 14). If the suite grows past that, put the
        // two Products in a Category made for this test and filter by it.
        await VisitAsync("/");

        await Expect(CardFor(first.Name)).ToBeVisibleAsync();
        await Expect(CardFor(second.Name)).ToBeVisibleAsync();
        await Expect(CardFor(first.Name).GetByRole(AriaRole.Link, new() { Name = firstShop.ShopName })).ToBeVisibleAsync();
        await Expect(CardFor(second.Name).GetByRole(AriaRole.Link, new() { Name = secondShop.ShopName })).ToBeVisibleAsync();
    }

    // US2 #2
    [Fact]
    public async Task A_visitor_finds_a_product_by_a_distinctive_word()
    {
        var (token, _) = await NewMerchantAsync();
        var teapot = await PublishedProductAsync(token, TestRun.Unique("Zephyrine teapot"));
        var saucer = await PublishedProductAsync(token, TestRun.Unique("Plain saucer"));

        // Both are listed before searching, so the checks below see search results, not the home listing.
        await VisitAsync("/");
        await Expect(CardFor(saucer.Name)).ToBeVisibleAsync();
        await Expect(CardFor(teapot.Name)).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Searchbox, new() { Name = "Search Products" }).FillAsync("Zephyrine");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();

        await Expect(CardFor(saucer.Name)).ToHaveCountAsync(0);
        await Expect(CardFor(teapot.Name)).ToBeVisibleAsync();
        await Expect(Cards()).ToHaveCountAsync(1);
    }

    // US2 #3
    [Fact]
    public async Task A_visitor_sees_a_products_variants_availability_and_cover_image_but_never_stock()
    {
        var (token, shop) = await NewMerchantAsync();
        var product = await Fixture.CreateProductAsync(
            token,
            TestRun.Unique("Colour mug"),
            [new OptionInput("Colour", ["Red", "Blue"])],
            [new VariantInput(["Red"], 120_000, 5), new VariantInput(["Blue"], 150_000, 2)],
            Ct);
        product = await Fixture.UploadImageAsync(token, product, TestImages.Png, Ct);
        await Fixture.PublishAsync(token, product, Ct);

        await VisitAsync($"/products/{product.Id}");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = product.Name })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = shop.ShopName })).ToBeVisibleAsync();

        // The cover is the first image: it carries the Product name and really rendered (8×8 PNG).
        var cover = Page.GetByRole(AriaRole.Img, new() { Name = product.Name });
        await Expect(cover).ToBeVisibleAsync();
        await Expect(cover).ToHaveJSPropertyAsync("naturalWidth", 8);
        await Expect(Page.GetByLabel("No image")).ToHaveCountAsync(0);

        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Availability" })).ToBeVisibleAsync();
        await Expect(VariantRow("Red")).ToContainTextAsync("120,000");
        await Expect(VariantRow("Red").GetByRole(AriaRole.Cell).Last).ToHaveTextAsync("5");
        await Expect(VariantRow("Blue")).ToContainTextAsync("150,000");
        await Expect(VariantRow("Blue").GetByRole(AriaRole.Cell).Last).ToHaveTextAsync("2");

        // Buyers see Availability, never stock.
        await Expect(Page.GetByRole(AriaRole.Columnheader, new() { Name = "Stock" })).ToHaveCountAsync(0);
        await Expect(Page.GetByText("Stock")).ToHaveCountAsync(0);
    }

    // US2 #4
    [Fact]
    public async Task A_visitor_follows_a_product_to_its_shop_page()
    {
        var (token, shop) = await NewMerchantAsync();
        var first = await PublishedProductAsync(token, TestRun.Unique("Bowl"));
        var second = await PublishedProductAsync(token, TestRun.Unique("Plate"));

        await VisitAsync($"/products/{first.Id}");
        await Page.GetByRole(AriaRole.Link, new() { Name = shop.ShopName }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = shop.ShopName })).ToBeVisibleAsync();
        await Expect(CardFor(first.Name)).ToBeVisibleAsync();
        await Expect(CardFor(second.Name)).ToBeVisibleAsync();
    }

    // US2 #5
    [Fact]
    public async Task A_visitor_never_sees_draft_or_removed_products()
    {
        var (token, shop) = await NewMerchantAsync();
        var draft = await Fixture.CreateProductAsync(token, TestRun.Unique("Quillback draft jug"), null, null, Ct);
        var removed = await PublishedProductAsync(token, TestRun.Unique("Quillback removed jug"));
        await Fixture.RemoveAsync(await Accounts.TokenAsync(Accounts.Operator, Ct), removed, Ct);
        // A Published Product of the same Shop, so the pages below are known to have loaded their listing.
        var visible = await PublishedProductAsync(token, TestRun.Unique("Visible jug"));

        // Home listing and search: neither appears.
        await VisitAsync("/");
        await Expect(CardFor(visible.Name)).ToBeVisibleAsync();
        await Expect(CardFor(draft.Name)).ToHaveCountAsync(0);
        await Expect(CardFor(removed.Name)).ToHaveCountAsync(0);

        await Page.GetByRole(AriaRole.Searchbox, new() { Name = "Search Products" }).FillAsync("Quillback");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(CardFor(visible.Name)).ToHaveCountAsync(0);
        await Expect(Page.GetByText("No Products found.")).ToBeVisibleAsync();

        // Shop page: only the Published Product appears.
        await Page.GotoAsync($"/shops/{shop.MerchantId}");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = shop.ShopName })).ToBeVisibleAsync();
        await Expect(CardFor(visible.Name)).ToBeVisibleAsync();
        await Expect(CardFor(draft.Name)).ToHaveCountAsync(0);
        await Expect(CardFor(removed.Name)).ToHaveCountAsync(0);

        // Direct address: the Product is not available, and its name is never shown.
        foreach (var product in new[] { draft, removed })
        {
            await Page.GotoAsync($"/products/{product.Id}");
            await Expect(Page.GetByText("This Product is not available.")).ToBeVisibleAsync();
            await Expect(Page.GetByText(product.Name)).ToHaveCountAsync(0);
        }
    }

    private ILocator Cards() => Page.GetByRole(AriaRole.Listitem);

    private ILocator CardFor(string productName) => Cards().Filter(new() { HasText = productName });

    private ILocator VariantRow(string optionValues) =>
        Page.GetByRole(AriaRole.Row).Filter(new() { Has = Page.GetByRole(AriaRole.Cell, new() { Name = optionValues, Exact = true }) });

    /// <summary>A fresh Account that owns a newly opened Merchant, and that Account's token.</summary>
    private async Task<(string Token, OpenedMerchant Merchant)> NewMerchantAsync()
    {
        var account = await Accounts.CreateAsync("merchant", Ct);
        var token = await Accounts.TokenAsync(account, Ct);
        return (token, await Fixture.OpenMerchantAsync(token, Ct));
    }

    private async Task<CreatedProduct> PublishedProductAsync(string token, string name)
    {
        var product = await Fixture.CreateProductAsync(token, name, null, null, Ct);
        return await Fixture.PublishAsync(token, product, Ct);
    }
}
