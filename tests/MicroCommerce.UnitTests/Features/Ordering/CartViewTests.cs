using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Ordering;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Ordering;

public class CartViewTests
{
    private readonly AccountId _accountId = AccountId.New();
    private readonly MerchantId _shopId = MerchantId.New();

    private CartLine Line(int quantity) =>
        new() { AccountId = _accountId, VariantId = VariantId.New(), Quantity = quantity };

    private VariantLookup Lookup(CartLine line, long price = 100_000, int available = 10, bool buyable = true, MerchantId? merchantId = null) =>
        new(line.VariantId, ProductId.New(), "Mug", ["Red"], merchantId ?? _shopId, "Mug Corner", Money.From(price), 350, available, buyable);

    private static CartResponse View(IEnumerable<CartLine> lines, IEnumerable<VariantLookup> lookups, MerchantId? ownMerchantId = null) =>
        CartView.Build([.. lines], lookups.ToDictionary(l => l.VariantId), ownMerchantId);

    [Fact]
    public void Buyable_lines_show_live_data_and_add_up()
    {
        var mug = Line(2);
        var bowl = Line(1);

        var cart = View([mug, bowl], [Lookup(mug, price: 120_000), Lookup(bowl, price: 50_000)]);

        cart.Lines[0].ShouldBeEquivalentTo(new CartLineResponse(
            mug.VariantId, 2, cart.Lines[0].ProductId, "Mug", ["Red"], _shopId, "Mug Corner",
            Money.From(120_000), Money.From(240_000), 10, new List<CartLineFlag>()));
        cart.Lines.Select(l => l.VariantId).ShouldBe([mug.VariantId, bowl.VariantId]);
        cart.ItemsTotal.ShouldBe(Money.From(290_000));
    }

    [Fact]
    public void Empty_cart_totals_zero()
    {
        var cart = View([], []);

        cart.Lines.ShouldBeEmpty();
        cart.ItemsTotal.ShouldBe(Money.From(0));
    }

    [Fact]
    public void Variant_that_no_longer_exists_is_unavailable_and_not_counted()
    {
        var gone = Line(1);
        var mug = Line(1);

        var cart = View([gone, mug], [Lookup(mug)]);

        var line = cart.Lines[0];
        line.VariantId.ShouldBe(gone.VariantId);
        line.Quantity.ShouldBe(1);
        line.ProductName.ShouldBeNull();
        line.UnitPrice.ShouldBeNull();
        line.LineTotal.ShouldBeNull();
        line.Flags.ShouldBe([CartLineFlag.Unavailable]);
        cart.ItemsTotal.ShouldBe(Money.From(100_000));
    }

    [Fact]
    public void Variant_that_is_not_buyable_keeps_its_details_but_is_unavailable_and_not_counted()
    {
        var hidden = Line(1);

        var cart = View([hidden], [Lookup(hidden, buyable: false)]);

        var line = cart.Lines.ShouldHaveSingleItem();
        line.ProductName.ShouldBe("Mug");
        line.UnitPrice.ShouldBe(Money.From(100_000));
        line.LineTotal.ShouldBeNull();
        line.Flags.ShouldBe([CartLineFlag.Unavailable]);
        cart.ItemsTotal.ShouldBe(Money.From(0));
    }

    [Fact]
    public void Quantity_above_available_is_flagged_but_still_counted()
    {
        var mug = Line(3);

        var cart = View([mug], [Lookup(mug, available: 2)]);

        cart.Lines.ShouldHaveSingleItem().Flags.ShouldBe([CartLineFlag.InsufficientStock]);
        cart.ItemsTotal.ShouldBe(Money.From(300_000));
    }

    [Fact]
    public void Own_merchants_variant_is_flagged()
    {
        var mine = Line(1);
        var theirs = Line(1);
        var myMerchant = MerchantId.New();

        var cart = View([mine, theirs], [Lookup(mine, merchantId: myMerchant), Lookup(theirs)], myMerchant);

        cart.Lines[0].Flags.ShouldBe([CartLineFlag.OwnMerchant]);
        cart.Lines[1].Flags.ShouldBeEmpty();
    }

    [Fact]
    public void Line_that_takes_the_total_past_the_safe_maximum_is_flagged_not_thrown()
    {
        var first = Line(1);
        var second = Line(99);
        var third = Line(1);

        var cart = View([first, second, third],
            [Lookup(first, price: Product.MaxPrice), Lookup(second, price: Product.MaxPrice, available: 99), Lookup(third, price: 0)]);

        cart.Lines[0].Flags.ShouldBeEmpty();
        cart.Lines[1].Flags.ShouldBe([CartLineFlag.TotalTooLarge]);
        cart.Lines[2].Flags.ShouldBeEmpty();
        cart.Lines[1].LineTotal.ShouldBeNull();
        cart.ItemsTotal.ShouldBe(Money.From(Product.MaxPrice));
    }
}
