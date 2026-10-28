using FluentValidation.Results;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class ProductValidatorTests
{
    private readonly CreateProductValidator _create = new();
    private readonly UpdateProductValidator _update = new();

    private static VariantRequest Variant(params string[] optionValues) => new(optionValues, 120_000, 5, 350);

    private static readonly CreateProductRequest Valid =
        new("Coffee Mug", "Stoneware.", CategoryId.New(), [], [Variant()]);

    private static readonly CreateProductRequest WithOptions = Valid with
    {
        Options = [new("Colour", ["Red", "Blue"]), new("Size", ["S", "M"])],
        Variants = [Variant("Red", "S"), Variant("Red", "M"), Variant("Blue", "S"), Variant("Blue", "M")],
    };

    private List<ValidationFailure> Validate(CreateProductRequest request)
    {
        var update = new UpdateProductRequest(
            ProductId.New(), request.Name, request.Description, request.CategoryId, request.Options, request.Variants);
        var createFailures = _create.Validate(request).Errors;

        // Creating and editing share their rules.
        _update.Validate(update).Errors.Select(e => (e.PropertyName, e.ErrorMessage))
            .ShouldBe(createFailures.Select(e => (e.PropertyName, e.ErrorMessage)));
        return createFailures;
    }

    private IEnumerable<string> Failures(CreateProductRequest request) => Validate(request).Select(e => e.PropertyName);

    [Fact]
    public void Valid_request_passes()
    {
        Failures(Valid).ShouldBeEmpty();
    }

    [Fact]
    public void Description_and_options_are_optional_and_stock_may_be_zero()
    {
        Failures(Valid with { Description = null, Options = null, Variants = [Variant() with { Stock = 0 }] }).ShouldBeEmpty();
        Failures(Valid with { Variants = [Variant() with { OptionValues = null }] }).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_name_fails(string name)
    {
        Failures(Valid with { Name = name }).ShouldBe([nameof(CreateProductRequest.Name)]);
    }

    [Fact]
    public void Name_of_200_characters_passes_and_201_fails()
    {
        Failures(Valid with { Name = new string('a', 200) }).ShouldBeEmpty();
        Failures(Valid with { Name = new string('a', 201) }).ShouldBe([nameof(CreateProductRequest.Name)]);
    }

    [Fact]
    public void Description_over_5000_characters_fails()
    {
        Failures(Valid with { Description = new string('a', 5001) }).ShouldBe([nameof(CreateProductRequest.Description)]);
    }

    [Fact]
    public void Missing_category_fails()
    {
        Failures(Valid with { CategoryId = null }).ShouldBe([nameof(CreateProductRequest.CategoryId)]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_price_fails(long price)
    {
        Failures(Valid with { Variants = [Variant() with { Price = price }] }).ShouldBe(["Variants[0].Price"]);
    }

    [Fact]
    public void Price_up_to_javascripts_max_safe_integer_passes_and_above_fails()
    {
        Failures(Valid with { Variants = [Variant() with { Price = 9_007_199_254_740_991 }] }).ShouldBeEmpty();
        Failures(Valid with { Variants = [Variant() with { Price = 9_007_199_254_740_992 }] }).ShouldBe(["Variants[0].Price"]);
    }

    [Fact]
    public void Negative_stock_fails()
    {
        Failures(Valid with { Variants = [Variant() with { Stock = -1 }] }).ShouldBe(["Variants[0].Stock"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_weight_fails(int weight)
    {
        Failures(Valid with { Variants = [Variant() with { WeightGrams = weight }] }).ShouldBe(["Variants[0].WeightGrams"]);
    }

    [Fact]
    public void Every_variant_is_checked()
    {
        var variants = WithOptions.Variants.ToList();
        variants[2] = variants[2] with { Price = 0, Stock = -1 };

        Failures(WithOptions with { Variants = variants }).ShouldBe(["Variants[2].Price", "Variants[2].Stock"]);
    }

    [Fact]
    public void Missing_variants_fail()
    {
        Failures(Valid with { Variants = [] }).ShouldBe([nameof(CreateProductRequest.Variants)]);
    }

    [Fact]
    public void Two_options_with_every_combination_pass()
    {
        Failures(WithOptions).ShouldBeEmpty();
    }

    [Fact]
    public void Option_values_match_ignoring_case_and_surrounding_spaces()
    {
        Failures(WithOptions with
        {
            Variants = [Variant(" red", "s "), Variant("RED", "M"), Variant("Blue", "S"), Variant("blue", "m")],
        }).ShouldBeEmpty();
    }

    [Fact]
    public void More_than_two_options_fail()
    {
        Failures(WithOptions with { Options = [.. WithOptions.Options!, new("Material", ["Cotton"])] })
            .ShouldBe([nameof(CreateProductRequest.Options)]);
    }

    [Fact]
    public void Option_needs_a_name_and_values()
    {
        Failures(WithOptions with { Options = [new(" ", ["Red", "Blue"]), new("Size", [])] })
            .ShouldBe(["Options[0].Name", "Options[1].Values"]);
    }

    [Fact]
    public void Option_without_values_fails()
    {
        Failures(WithOptions with { Options = [new("Colour", null!), WithOptions.Options![1]] }).ShouldBe(["Options[0].Values"]);
    }

    [Fact]
    public void Blank_or_too_long_option_values_fail()
    {
        Failures(WithOptions with { Options = [new("Colour", ["Red", " "]), new("Size", ["S", new string('M', 51)])] })
            .ShouldBe(["Options[0].Values[1]", "Options[1].Values[1]"]);
    }

    [Fact]
    public void Option_names_and_values_cannot_contain_control_characters()
    {
        var failures = Validate(WithOptions with { Options = [new("Colour\u001e", ["Red", "Blue\nGreen"]), WithOptions.Options![1]] });

        failures.Select(e => (e.PropertyName, e.ErrorMessage)).ShouldBe([
            ("Options[0].Name", "Can't contain line breaks or other control characters."),
            ("Options[0].Values[1]", "Can't contain line breaks or other control characters."),
        ]);
    }

    [Fact]
    public void Option_values_must_differ_ignoring_case()
    {
        var failure = Validate(WithOptions with { Options = [new("Colour", ["Red", " red"]), WithOptions.Options![1]] })
            .ShouldHaveSingleItem();

        failure.PropertyName.ShouldBe("Options[0].Values");
        failure.ErrorMessage.ShouldBe("Each value of an option must be different.");
    }

    [Fact]
    public void Option_names_must_differ_ignoring_case()
    {
        var failure = Validate(WithOptions with { Options = [new("Colour", ["Red", "Blue"]), new("colour ", ["S", "M"])] })
            .ShouldHaveSingleItem();

        failure.PropertyName.ShouldBe(nameof(CreateProductRequest.Options));
        failure.ErrorMessage.ShouldBe("Each option needs a different name.");
    }

    [Fact]
    public void An_option_can_have_up_to_twenty_values()
    {
        string[] values = [.. Enumerable.Range(1, 21).Select(i => $"V{i}")];
        var twenty = WithOptions with { Options = [new("Pack", values[..20])], Variants = [.. values[..20].Select(v => Variant(v))] };

        Failures(twenty).ShouldBeEmpty();
        Failures(twenty with { Options = [new("Pack", values)] }).ShouldBe(["Options[0].Values"]);
    }

    [Fact]
    public void A_missing_combination_fails()
    {
        var failure = Validate(WithOptions with { Variants = WithOptions.Variants.SkipLast(1).ToList() }).ShouldHaveSingleItem();

        failure.PropertyName.ShouldBe(nameof(CreateProductRequest.Variants));
        failure.ErrorMessage.ShouldBe("Missing a Variant for Blue / M.");
    }

    [Fact]
    public void A_duplicate_combination_fails()
    {
        var failures = Validate(WithOptions with { Variants = [.. WithOptions.Variants.SkipLast(1), Variant("red", "S")] });

        failures.Select(e => (e.PropertyName, e.ErrorMessage)).ShouldBe([
            ("Variants[3].OptionValues", "Another Variant has the same option values."),
            ("Variants", "Missing a Variant for Blue / M."),
        ]);
    }

    [Fact]
    public void An_unknown_option_value_fails()
    {
        var failures = Validate(WithOptions with { Variants = [.. WithOptions.Variants.SkipLast(1), Variant("Green", "M")] });

        failures.Select(e => (e.PropertyName, e.ErrorMessage)).ShouldBe([
            ("Variants[3].OptionValues", "'Green' is not a value of Colour."),
            ("Variants", "Missing a Variant for Blue / M."),
        ]);
    }

    [Fact]
    public void A_variant_needs_one_value_for_each_option()
    {
        var failures = Validate(WithOptions with { Variants = [.. WithOptions.Variants.SkipLast(1), Variant("Blue")] });

        failures.Select(e => (e.PropertyName, e.ErrorMessage)).ShouldBe([
            ("Variants[3].OptionValues", "Give one value for each option: Colour, Size."),
            ("Variants", "Missing a Variant for Blue / M."),
        ]);
    }

    [Fact]
    public void Without_options_there_is_exactly_one_variant_without_option_values()
    {
        Validate(Valid with { Variants = [Variant(), Variant()] }).Select(e => (e.PropertyName, e.ErrorMessage))
            .ShouldBe([("Variants", "A Product without options has exactly one Variant.")]);
        Validate(Valid with { Variants = [Variant("Red")] }).Select(e => (e.PropertyName, e.ErrorMessage))
            .ShouldBe([("Variants[0].OptionValues", "A Product without options has a Variant without option values.")]);
    }

    [Fact]
    public void Combinations_are_not_checked_while_the_options_are_invalid()
    {
        Failures(WithOptions with { Options = [new("Colour", ["Red", "Red"]), WithOptions.Options![1]], Variants = [Variant()] })
            .ShouldBe(["Options[0].Values"]);
    }
}
