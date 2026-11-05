using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.UnitTests.Domain.Catalog;

public class ProductTests
{
    internal static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);
    internal static readonly CategoryId Kitchen = CategoryId.New();

    internal static VariantSpec Spec(long price = 120_000, int stock = 10, int weight = 400, params string[] values) =>
        new(values, Money.From(price), stock, weight);

    internal static Product Simple(string name = "Ceramic coffee mug", string? description = "350 ml", int stock = 10) =>
        Product.Create(ProductId.New(), MerchantId.New(), name, description, Kitchen, [], [Spec(stock: stock)], Now);

    [Fact]
    public void A_new_product_is_a_draft_at_version_1_with_one_default_variant()
    {
        var merchant = MerchantId.New();

        var product = Product.Create(ProductId.New(), merchant, "  Ceramic coffee mug ", "350 ml", Kitchen, [], [Spec()], Now);

        product.MerchantId.ShouldBe(merchant);
        product.Name.ShouldBe("Ceramic coffee mug");
        product.Description.ShouldBe("350 ml");
        product.CategoryId.ShouldBe(Kitchen);
        product.Status.ShouldBe(ProductStatus.Draft);
        product.Version.ShouldBe(1);
        product.CreatedAt.ShouldBe(Now);
        product.Options.ShouldBeEmpty();
        product.Images.ShouldBeEmpty();
        var variant = product.Variants.ShouldHaveSingleItem();
        variant.OptionValues.ShouldBeEmpty();
        variant.Price.ShouldBe(Money.From(120_000));
        variant.Stock.ShouldBe(10);
        variant.WeightGrams.ShouldBe(400);
    }

    [Fact]
    public void Publishing_and_unpublishing_move_the_status_and_the_version()
    {
        var product = Simple();

        product.Publish();
        product.Status.ShouldBe(ProductStatus.Published);
        product.Version.ShouldBe(2);

        product.Unpublish();
        product.Status.ShouldBe(ProductStatus.Draft);
        product.Version.ShouldBe(3);
    }

    [Fact]
    public void Publishing_twice_or_unpublishing_a_draft_changes_nothing()
    {
        var product = Simple();
        product.Unpublish();
        product.Version.ShouldBe(1);

        product.Publish();
        product.Publish();

        product.Status.ShouldBe(ProductStatus.Published);
        product.Version.ShouldBe(2);
    }

    [Fact]
    public void Editing_keeps_the_status_and_the_default_variant_and_bumps_the_version()
    {
        var product = Simple();
        product.Publish();
        var variantId = product.Variants.Single().Id;
        var bedding = CategoryId.New();

        product.Edit("Big mug", null, bedding, [], [Spec(price: 150_000, stock: 3)]);

        product.Name.ShouldBe("Big mug");
        product.Description.ShouldBeNull();
        product.CategoryId.ShouldBe(bedding);
        product.Status.ShouldBe(ProductStatus.Published);
        product.Version.ShouldBe(3);
        var variant = product.Variants.ShouldHaveSingleItem();
        variant.Id.ShouldBe(variantId);
        variant.Price.ShouldBe(Money.From(150_000));
        variant.Stock.ShouldBe(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void A_blank_name_is_refused(string name) => Should.Throw<DomainRuleViolation>(() => Simple(name: name));

    [Fact]
    public void A_name_is_at_most_200_characters()
    {
        Simple(name: new string('n', 200)).Name.Length.ShouldBe(200);
        Should.Throw<DomainRuleViolation>(() => Simple(name: new string('n', 201)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_description_becomes_null(string? description) => Simple(description: description).Description.ShouldBeNull();

    [Fact]
    public void A_description_is_at_most_5000_characters() =>
        Should.Throw<DomainRuleViolation>(() => Simple(description: new string('d', 5001)));

    [Fact]
    public void Negative_stock_and_non_positive_weight_are_refused()
    {
        Should.Throw<DomainRuleViolation>(() => Simple(stock: -1));
        Should.Throw<DomainRuleViolation>(() =>
            Product.Create(ProductId.New(), MerchantId.New(), "Mug", null, Kitchen, [], [Spec(weight: 0)], Now));
    }

    [Fact]
    public void Zero_stock_is_allowed() => Simple(stock: 0).Variants.Single().Stock.ShouldBe(0);
}
