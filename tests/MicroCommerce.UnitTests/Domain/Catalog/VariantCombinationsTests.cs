using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.SharedKernel;
using static MicroCommerce.UnitTests.Domain.Catalog.ProductTests;

namespace MicroCommerce.UnitTests.Domain.Catalog;

public class VariantCombinationsTests
{
    private static readonly ProductOption Colour = new("Colour", ["Red", "Blue"]);
    private static readonly ProductOption Size = new("Size", ["S", "M", "L"]);

    private static VariantSpec[] SpecsFor(params ProductOption[] options) =>
        VariantCombinations.Expand(options).Select((values, i) => Spec(price: 100_000 + i, values: [.. values])).ToArray();

    private static Product Create(params ProductOption[] options) =>
        Product.Create(ProductId.New(), MerchantId.New(), "Tee", null, Kitchen, options, SpecsFor(options), Now);

    [Fact]
    public void Expansion_varies_the_last_option_fastest() =>
        VariantCombinations.Expand([Colour, Size]).Select(VariantCombinations.Describe)
            .ShouldBe(["Red / S", "Red / M", "Red / L", "Blue / S", "Blue / M", "Blue / L"]);

    [Fact]
    public void Without_options_there_is_one_empty_combination() =>
        VariantCombinations.Expand([]).ShouldHaveSingleItem().ShouldBeEmpty();

    [Fact]
    public void Two_options_give_every_combination_in_order()
    {
        var product = Create(Colour, Size);

        product.Variants.Count.ShouldBe(6);
        product.Variants.Select(v => string.Join('/', v.OptionValues)).ShouldBe(["Red/S", "Red/M", "Red/L", "Blue/S", "Blue/M", "Blue/L"]);
        product.Variants.Select(v => v.Position).ShouldBe([0, 1, 2, 3, 4, 5]);
    }

    [Fact]
    public void The_key_ignores_option_order_and_letter_case() =>
        VariantCombinations.Key([Colour, Size], ["Red", "S"]).ShouldBe(VariantCombinations.Key([Size, Colour], ["s", "RED"]));

    [Fact]
    public void Swapping_the_options_keeps_every_variant_id()
    {
        var product = Create(Colour, Size);
        var ids = product.Variants.ToDictionary(v => $"{v.OptionValues[0]}/{v.OptionValues[1]}", v => v.Id);

        product.Edit("Tee", null, Kitchen, [Size, Colour], SpecsFor(Size, Colour));

        product.Variants.ShouldAllBe(v => ids[$"{v.OptionValues[1]}/{v.OptionValues[0]}"] == v.Id);
    }

    [Fact]
    public void Removing_a_value_drops_exactly_its_variants_and_keeps_the_others()
    {
        var product = Create(Colour, Size);
        var before = product.Variants.ToDictionary(v => string.Join('/', v.OptionValues), v => v.Id);
        var smaller = new ProductOption("Size", ["S", "M"]);

        product.Edit("Tee", null, Kitchen, [Colour, smaller], SpecsFor(Colour, smaller));

        product.Variants.Count.ShouldBe(4);
        product.Variants.ShouldAllBe(v => before[string.Join('/', v.OptionValues)] == v.Id);
    }

    [Fact]
    public void Adding_a_value_adds_new_variants_with_new_ids()
    {
        var product = Create(Colour, Size);
        var before = product.Variants.Select(v => v.Id).ToHashSet();
        var bigger = new ProductOption("Size", ["S", "M", "L", "XL"]);

        product.Edit("Tee", null, Kitchen, [Colour, bigger], SpecsFor(Colour, bigger));

        product.Variants.Count.ShouldBe(8);
        product.Variants.Where(v => v.OptionValues[1] == "XL").ShouldAllBe(v => !before.Contains(v.Id));
    }

    [Fact]
    public void Removing_every_option_leaves_one_default_variant()
    {
        var product = Create(Colour);

        product.Edit("Tee", null, Kitchen, [], [Spec()]);

        product.Variants.ShouldHaveSingleItem().OptionValues.ShouldBeEmpty();
        product.Options.ShouldBeEmpty();
    }

    [Fact]
    public void Values_are_spelt_as_the_option_spells_them()
    {
        var product = Product.Create(ProductId.New(), MerchantId.New(), "Tee", null, Kitchen, [Colour],
            [Spec(values: "red"), Spec(values: "BLUE")], Now);

        product.Variants.Select(v => v.OptionValues.Single()).ShouldBe(["Red", "Blue"]);
    }

    [Fact]
    public void Values_with_surrounding_spaces_match_their_option_value()
    {
        var product = Product.Create(ProductId.New(), MerchantId.New(), "Tee", null, Kitchen, [Colour],
            [Spec(values: " Red "), Spec(values: "Blue")], Now);

        product.Variants.Select(v => v.OptionValues.Single()).ShouldBe(["Red", "Blue"]);
    }

    [Fact]
    public void Missing_duplicate_and_unknown_combinations_are_refused()
    {
        Should.Throw<DomainRuleViolation>(() => Product.Create(ProductId.New(), MerchantId.New(), "Tee", null, Kitchen, [Colour], [Spec(values: "Red")], Now))
            .Message.ShouldContain("Missing");
        Should.Throw<DomainRuleViolation>(() => Product.Create(ProductId.New(), MerchantId.New(), "Tee", null, Kitchen, [Colour],
            [Spec(values: "Red"), Spec(values: "red"), Spec(values: "Blue")], Now)).Message.ShouldContain("Duplicate");
        Should.Throw<DomainRuleViolation>(() => Product.Create(ProductId.New(), MerchantId.New(), "Tee", null, Kitchen, [Colour],
            [Spec(values: "Red"), Spec(values: "Green")], Now)).Message.ShouldContain("Unknown");
    }

    [Fact]
    public void Option_rules_are_enforced()
    {
        Should.Throw<DomainRuleViolation>(() => new ProductOption("", ["Red"]));
        Should.Throw<DomainRuleViolation>(() => new ProductOption(new string('n', 51), ["Red"]));
        Should.Throw<DomainRuleViolation>(() => new ProductOption("Col\tour", ["Red"]));
        Should.Throw<DomainRuleViolation>(() => new ProductOption("Colour", []));
        Should.Throw<DomainRuleViolation>(() => new ProductOption("Colour", Enumerable.Range(0, 21).Select(i => $"v{i}").ToList()));
        Should.Throw<DomainRuleViolation>(() => new ProductOption("Colour", ["Red", "red"]));
        Should.Throw<DomainRuleViolation>(() => new ProductOption("Colour", [new string('v', 51)]));
        Should.Throw<DomainRuleViolation>(() => ProductOption.CheckSet([Colour, Size, new ProductOption("Fit", ["Slim"])]));
        Should.Throw<DomainRuleViolation>(() => ProductOption.CheckSet([Colour, new ProductOption("colour", ["Green"])]));
    }
}
