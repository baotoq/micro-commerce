using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class ProductTests
{
    private static readonly ProductOption Colour = new("Colour", ["Red", "Blue"]);
    private static readonly ProductOption Size = new("Size", ["S", "M"]);

    private static Product NewProduct() =>
        new() { MerchantId = MerchantId.New(), Name = "T-Shirt", CategoryId = CategoryId.New() };

    private static VariantDetails Details(long price, params string[] optionValues) =>
        new(optionValues, Money.From(price), 5, 200);

    private static Dictionary<string, VariantId> IdsByLabel(Product product) =>
        product.Variants.ToDictionary(v => string.Join(" / ", v.OptionValues), v => v.Id);

    [Fact]
    public void New_product_is_a_draft_without_options_or_variants_and_gets_a_version_7_id()
    {
        var product = NewProduct();

        product.Status.ShouldBe(ProductStatus.Draft);
        product.Options.ShouldBeEmpty();
        product.Variants.ShouldBeEmpty();
        product.Id.Value.Version.ShouldBe(7);
    }

    [Fact]
    public void Without_options_a_product_has_a_single_default_variant()
    {
        var product = NewProduct();

        product.SetVariants([], [new([], Money.From(120_000), 5, 350)]);

        var variant = product.Variants.ShouldHaveSingleItem();
        variant.Id.Value.Version.ShouldBe(7);
        variant.OptionValues.ShouldBeEmpty();
        (variant.Price, variant.Stock, variant.WeightGrams).ShouldBe((Money.From(120_000), 5, 350));
    }

    [Fact]
    public void Setting_the_default_variant_again_keeps_its_id()
    {
        var product = NewProduct();
        product.SetVariants([], [new([], Money.From(120_000), 5, 350)]);
        var id = product.Variants.Single().Id;

        product.SetVariants([], [new([], Money.From(150_000), 0, 400)]);

        var variant = product.Variants.ShouldHaveSingleItem();
        variant.Id.ShouldBe(id);
        (variant.Price, variant.Stock, variant.WeightGrams).ShouldBe((Money.From(150_000), 0, 400));
    }

    [Fact]
    public void Two_options_give_a_variant_per_combination_in_option_order()
    {
        var product = NewProduct();

        product.SetVariants([Colour, Size],
            [Details(4, "Blue", "M"), Details(1, "Red", "S"), Details(3, "Blue", "S"), Details(2, "Red", "M")]);

        product.Options.ShouldBe([Colour, Size]);
        product.Variants.Select(v => (string.Join(" / ", v.OptionValues), v.Position, v.Price.Value)).ShouldBe([
            ("Red / S", 0, 1L), ("Red / M", 1, 2L), ("Blue / S", 2, 3L), ("Blue / M", 3, 4L),
        ]);
        product.Variants.Select(v => v.Id).Distinct().Count().ShouldBe(4);
    }

    [Fact]
    public void Editing_keeps_the_ids_of_surviving_combinations_only()
    {
        var product = NewProduct();
        product.SetVariants([Colour, Size],
            [Details(1, "Red", "S"), Details(1, "Red", "M"), Details(1, "Blue", "S"), Details(1, "Blue", "M")]);
        var before = IdsByLabel(product);

        product.SetVariants([new("Colour", ["Red", "Green"]), Size],
            [Details(9, "Red", "S"), Details(9, "Red", "M"), Details(9, "Green", "S"), Details(9, "Green", "M")]);

        var after = IdsByLabel(product);
        after.Keys.ShouldBe(["Red / S", "Red / M", "Green / S", "Green / M"]);
        after["Red / S"].ShouldBe(before["Red / S"]);
        after["Red / M"].ShouldBe(before["Red / M"]);
        after["Green / S"].ShouldNotBeOneOf([.. before.Values]);
        after["Green / M"].ShouldNotBeOneOf([.. before.Values]);
        product.Variants.ShouldAllBe(v => v.Price == Money.From(9));
    }

    [Fact]
    public void Reordering_options_or_changing_their_case_keeps_the_variant_ids()
    {
        var product = NewProduct();
        product.SetVariants([Colour, Size],
            [Details(1, "Red", "S"), Details(1, "Red", "M"), Details(1, "Blue", "S"), Details(1, "Blue", "M")]);
        var before = IdsByLabel(product);

        product.SetVariants([new("size", ["s", "m"]), new("colour", ["red", "blue"])],
            [Details(1, "s", "red"), Details(1, "s", "blue"), Details(1, "m", "red"), Details(1, "m", "blue")]);

        var after = IdsByLabel(product);
        after["s / red"].ShouldBe(before["Red / S"]);
        after["s / blue"].ShouldBe(before["Blue / S"]);
        after["m / red"].ShouldBe(before["Red / M"]);
        after["m / blue"].ShouldBe(before["Blue / M"]);
    }

    [Fact]
    public void Adding_the_first_option_replaces_the_default_variant()
    {
        var product = NewProduct();
        product.SetVariants([], [Details(1)]);
        var defaultId = product.Variants.Single().Id;

        product.SetVariants([Colour], [Details(1, "Red"), Details(1, "Blue")]);

        product.Variants.Select(v => v.OptionValues.Single()).ShouldBe(["Red", "Blue"]);
        product.Variants.ShouldNotContain(v => v.Id == defaultId);
    }

    [Fact]
    public void Variants_that_are_not_exactly_the_combinations_are_refused()
    {
        var product = NewProduct();

        Should.Throw<ArgumentException>(() => product.SetVariants([Colour], [Details(1, "Red")]));
        Should.Throw<ArgumentException>(() => product.SetVariants([Colour], [Details(1, "Red"), Details(1, "Green")]));
        Should.Throw<ArgumentException>(() => product.SetVariants([Colour], [Details(1, "Red"), Details(1, "Red"), Details(1, "Blue")]));
        Should.Throw<ArgumentException>(() => product.SetVariants([], [Details(1), Details(2)]));
        Should.Throw<ArgumentException>(() => product.SetVariants(
            [Colour, Size, new("Material", ["Cotton"])], []));
        product.Variants.ShouldBeEmpty();
    }

    [Fact]
    public void Publish_and_unpublish_move_between_draft_and_published()
    {
        var product = NewProduct();

        product.Publish();
        product.Status.ShouldBe(ProductStatus.Published);
        product.Publish();
        product.Status.ShouldBe(ProductStatus.Published);

        product.Unpublish();
        product.Status.ShouldBe(ProductStatus.Draft);
    }
}
