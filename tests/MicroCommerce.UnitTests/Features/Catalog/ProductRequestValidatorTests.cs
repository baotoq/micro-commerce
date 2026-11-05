using MicroCommerce.ApiService.Features.Catalog.MerchantProducts;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class ProductRequestValidatorTests
{
    private readonly CreateProductValidator _create = new();
    private readonly UpdateProductValidator _update = new();

    internal static VariantRequest Variant(long? price = 120_000, int? stock = 10, int? weight = 400, params string[] values) =>
        new([.. values], price, stock, weight);

    internal static CreateProductRequest Request(
        string? name = "Ceramic coffee mug",
        string? description = "350 ml",
        Guid? categoryId = null,
        List<OptionRequest>? options = null,
        List<VariantRequest>? variants = null) =>
        new(name!, description, categoryId ?? Guid.NewGuid(), options ?? [], variants ?? [Variant()]);

    [Fact]
    public void A_simple_product_is_valid() => _create.Validate(Request()).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_is_required(string? name) => Errors(Request(name: name)).ShouldBe(["Name"]);

    [Fact]
    public void Name_and_description_have_maximum_lengths()
    {
        Errors(Request(name: new string('n', 201))).ShouldBe(["Name"]);
        Errors(Request(description: new string('d', 5001))).ShouldBe(["Description"]);
    }

    [Fact]
    public void Category_is_required() => Errors(Request(categoryId: Guid.Empty)).ShouldBe(["CategoryId"]);

    [Theory]
    [InlineData(0L)]
    [InlineData(-5L)]
    [InlineData(9_007_199_254_740_992L)]
    [InlineData(null)]
    public void Price_must_be_above_zero_and_javascript_safe(long? price) =>
        Errors(Request(variants: [Variant(price: price)])).ShouldBe(["Variants[0].Price"]);

    [Fact]
    public void Stock_must_not_be_negative_and_weight_must_be_positive()
    {
        Errors(Request(variants: [Variant(stock: -1)])).ShouldBe(["Variants[0].Stock"]);
        Errors(Request(variants: [Variant(weight: 0)])).ShouldBe(["Variants[0].WeightGrams"]);
    }

    [Fact]
    public void Without_options_there_is_exactly_one_variant_with_no_option_values()
    {
        Errors(Request(variants: [])).ShouldBe(["Variants"]);
        Errors(Request(variants: [Variant(), Variant()])).ShouldBe(["Variants"]);
        Errors(Request(variants: [Variant(values: "Red")])).ShouldBe(["Variants"]);
    }

    private static OptionRequest Option(string name, params string[] values) => new(name, [.. values]);

    private static List<VariantRequest> All(params string[][] combinations) => combinations.Select(c => Variant(values: c)).ToList();

    [Fact]
    public void Two_options_with_every_combination_are_valid() =>
        _create.Validate(Request(options: [Option("Colour", "Red", "Blue"), Option("Size", "S", "M")],
            variants: All(["Red", "S"], ["Red", "M"], ["Blue", "S"], ["Blue", "M"]))).IsValid.ShouldBeTrue();

    [Fact]
    public void Invalid_options_are_reported_on_options()
    {
        Errors(Request(options: [Option("A", "1"), Option("B", "1"), Option("C", "1")], variants: All(["1", "1", "1"]))).ShouldBe(["Options"]);
        Errors(Request(options: [Option("Colour")], variants: [])).ShouldBe(["Options"]);
        Errors(Request(options: [Option("Colour", "Red", "red")], variants: All(["Red"]))).ShouldBe(["Options"]);
        Errors(Request(options: [Option("Colour", "Red"), Option("colour", "Blue")], variants: All(["Red", "Blue"]))).ShouldBe(["Options"]);
    }

    [Fact]
    public void Missing_duplicate_and_unknown_combinations_are_reported_on_variants_by_name()
    {
        var colour = Option("Colour", "Red", "Blue");

        var missing = _create.Validate(Request(options: [colour], variants: All(["Red"])));
        missing.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldContain("Missing combination: Blue");

        var duplicate = _create.Validate(Request(options: [colour], variants: All(["Red"], ["red"], ["Blue"])));
        duplicate.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldContain("Duplicate combination: red");

        var unknown = _create.Validate(Request(options: [colour], variants: All(["Red"], ["Blue"], ["Green"])));
        unknown.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldContain("Unknown combination: Green");
        unknown.Errors.Single().PropertyName.ShouldBe("Variants");
    }

    [Fact]
    public void Missing_option_values_are_reported_on_the_variant_not_thrown()
    {
        var withoutValues = new VariantRequest(null!, 120_000, 10, 400);

        Errors(Request(variants: [withoutValues])).ShouldBe(["Variants[0].OptionValues"]);
        Errors(Request(options: [Option("Colour", "Red")], variants: [withoutValues])).ShouldBe(["Variants[0].OptionValues"]);
    }

    [Fact]
    public void Null_variants_and_options_are_reported_not_thrown()
    {
        Errors(Request(variants: [null!])).ShouldBe(["Variants[0]"]);
        Errors(Request(options: [null!], variants: [Variant()])).ShouldBe(["Options[0]"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void An_update_needs_the_version_it_was_based_on(int? version)
    {
        var request = Request();
        var update = new UpdateProductRequest(Guid.NewGuid(), request.Name, request.Description, request.CategoryId, request.Options, request.Variants, version);

        _update.Validate(update).Errors.Select(e => e.PropertyName).Distinct().ShouldBe(["Version"]);
    }

    internal List<string> Errors(CreateProductRequest request) =>
        _create.Validate(request).Errors.Select(e => e.PropertyName).Distinct().ToList();
}
