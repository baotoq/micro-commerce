using System.Text.Json.Serialization;
using FluentValidation;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// A listing a Merchant offers, in exactly one leaf Category. It is never bought directly; its Variants are: one
/// for every combination of its options' values, or a single default Variant when it has no options.
/// </summary>
public class Product
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 5000;

    /// <summary>JavaScript's <c>Number.MAX_SAFE_INTEGER</c>: a higher price would lose precision in the frontend.</summary>
    public const long MaxPrice = 9_007_199_254_740_991;

    public const int MaxOptions = 2;

    public ProductId Id { get; init; } = ProductId.New();

    /// <summary>The Merchant that owns the Product; every Merchant Centre query is scoped by it (ADR-0001).</summary>
    public required MerchantId MerchantId { get; init; }

    public required string Name { get; set; }
    public string? Description { get; set; }
    public required CategoryId CategoryId { get; set; }

    /// <summary>New Products start as Draft; edits keep the current status.</summary>
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    /// <summary>Up to <see cref="MaxOptions"/> options (e.g. Colour, Size), whose value combinations are the Variants.</summary>
    public List<ProductOption> Options { get; private set; } = [];

    /// <summary>One per combination of the options' values, in <see cref="Variant.Position"/> order once set.</summary>
    public List<Variant> Variants { get; } = [];

    public DateTimeOffset CreatedAt { get; init; } = DatabaseClock.UtcNow;

    public void Publish() => Status = ProductStatus.Published;

    public void Unpublish() => Status = ProductStatus.Draft;

    /// <summary>
    /// Replaces the options and Variants. <paramref name="variants"/> must hold every combination of the options'
    /// values exactly once, each Variant's values in option order (one Variant with no values when there are no
    /// options). A combination the Product already had keeps its Variant and ID, even if the options were reordered;
    /// the rest are new Variants, and combinations that are gone take their Variants with them.
    /// </summary>
    public void SetVariants(IReadOnlyList<ProductOption> options, IReadOnlyList<VariantDetails> variants)
    {
        if (options.Count > MaxOptions)
            throw new ArgumentException($"A Product can have at most {MaxOptions} options.", nameof(options));

        var combinations = VariantCombinations.Of(options);
        var detailsByKey = variants.ToDictionary(v => VariantCombinations.Key(options, v.OptionValues));
        if (detailsByKey.Count != combinations.Count
            || !combinations.All(c => detailsByKey.ContainsKey(VariantCombinations.Key(options, c))))
            throw new ArgumentException("The Variants must be exactly the combinations of the options' values.", nameof(variants));

        var existingByKey = Variants.ToDictionary(v => VariantCombinations.Key(Options, v.OptionValues));
        Variants.Clear();
        for (var position = 0; position < combinations.Count; position++)
        {
            var key = VariantCombinations.Key(options, combinations[position]);
            var details = detailsByKey[key];
            var variant = existingByKey.GetValueOrDefault(key)
                ?? new Variant { OptionValues = [], Price = details.Price, Stock = details.Stock, WeightGrams = details.WeightGrams };

            variant.Position = position;
            variant.OptionValues = [.. combinations[position]];
            variant.Price = details.Price;
            variant.Stock = details.Stock;
            variant.WeightGrams = details.WeightGrams;
            Variants.Add(variant);
        }

        Options = [.. options];
    }
}

/// <summary>
/// One of a Product's options, e.g. Colour with Red and Blue. Option names, and the values within an option, are
/// distinct ignoring case.
/// </summary>
public record ProductOption(string Name, IReadOnlyList<string> Values)
{
    public const int NameMaxLength = 50;
    public const int ValueMaxLength = 50;
    public const int MaxValues = 20;
}

/// <summary>A Variant's option values, in option order, and its price, stock and weight.</summary>
public record VariantDetails(IReadOnlyList<string> OptionValues, Money Price, int Stock, int WeightGrams);

/// <summary>
/// <c>Draft</c> → <c>Published</c> → <c>Draft</c>, moved by the Merchant. Buyers only ever see Published Products
/// (see <see cref="StorefrontQueries.VisibleProducts"/>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProductStatus>))]
public enum ProductStatus
{
    Draft,
    Published,
}

/// <summary>One purchasable combination of a Product's options, with its own price, stock and weight.</summary>
public class Variant
{
    public VariantId Id { get; init; } = VariantId.New();

    /// <summary>Its place among the Product's Variants: combinations in option order, the last option varying fastest.</summary>
    public int Position { get; set; }

    /// <summary>One value of each of the Product's options, in option order; empty for a default Variant.</summary>
    public required List<string> OptionValues { get; set; }

    public required Money Price { get; set; }

    /// <summary>Units available to sell; never negative.</summary>
    public required int Stock { get; set; }

    /// <summary>Parcel weight in grams, which the Carrier quotes Shipping Fees from; always positive.</summary>
    public required int WeightGrams { get; set; }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.MerchantId);
        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        // The database refuses to delete a Category holding Products too.
        builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder.ComplexCollection(p => p.Options, o => o.ToJson());

        // Variants are part of the Product: always loaded, saved and deleted with it.
        builder.OwnsMany(p => p.Variants, v =>
        {
            v.ToTable("Variants");
            v.WithOwner().HasForeignKey("ProductId");
            v.HasKey(x => x.Id);
            v.Property(x => x.Id).ValueGeneratedNever();
        });
    }
}

/// <summary>A Product as its Merchant sees it, in any status.</summary>
public record ProductResponse(
    ProductId Id,
    string Name,
    string? Description,
    CategoryId CategoryId,
    ProductStatus Status,
    IReadOnlyList<ProductOption> Options,
    IReadOnlyList<VariantResponse> Variants,
    DateTimeOffset CreatedAt);

/// <summary>A Variant with its option values in the Product's option order.</summary>
public record VariantResponse(VariantId Id, IReadOnlyList<string> OptionValues, Money Price, int Stock, int WeightGrams);

/// <summary>A row of the Merchant's Product list: its price range and total stock across Variants.</summary>
public record ProductSummaryResponse(
    ProductId Id,
    string Name,
    CategoryId CategoryId,
    ProductStatus Status,
    int VariantCount,
    Money MinPrice,
    Money MaxPrice,
    /// <summary>A <c>long</c>: each Variant's stock fits an <c>int</c>, but their sum may not.</summary>
    long TotalStock,
    DateTimeOffset CreatedAt);

/// <summary>A Product as Buyers see it on the storefront, with the Shop that sells it.</summary>
public record StorefrontProductResponse(
    ProductId Id,
    string Name,
    string? Description,
    CategoryId CategoryId,
    IReadOnlyList<ProductOption> Options,
    IReadOnlyList<StorefrontVariantResponse> Variants,
    MerchantId ShopId,
    string ShopName);

/// <summary>A Variant as Buyers see it: its option values in the Product's option order, price and available stock.</summary>
public record StorefrontVariantResponse(VariantId Id, IReadOnlyList<string> OptionValues, Money Price, int Stock);

/// <summary>A storefront list entry: its price range across Variants, whether any is in stock, and its Shop.</summary>
public record StorefrontProductSummaryResponse(
    ProductId Id,
    string Name,
    CategoryId CategoryId,
    Money MinPrice,
    Money MaxPrice,
    bool InStock,
    MerchantId ShopId,
    string ShopName);

/// <summary>What a Merchant gives when creating or editing a Product.</summary>
public interface IProductDetails
{
    string Name { get; }
    string? Description { get; }
    CategoryId? CategoryId { get; }

    /// <summary>Up to two options; none (or omitted) for a Product with a single default Variant.</summary>
    IReadOnlyList<ProductOption>? Options { get; }

    /// <summary>Exactly one per combination of the options' values; a single one without values when there are none.</summary>
    IReadOnlyList<VariantRequest> Variants { get; }
}

/// <summary>
/// A Variant as a Merchant gives it: one value of each option, in option order (none without options), and its price
/// in whole VND, stock and weight in grams.
/// </summary>
public record VariantRequest(IReadOnlyList<string>? OptionValues, long Price, int Stock, int WeightGrams);

public class ProductDetailsValidator : AbstractValidator<IProductDetails>
{
    public ProductDetailsValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(x => x.Description).MaximumLength(Product.DescriptionMaxLength);
        RuleFor(x => x.CategoryId).NotNull();

        RuleFor(x => x.Options)
            .Must(o => o!.Count <= Product.MaxOptions).WithMessage($"A Product can have at most {Product.MaxOptions} options.")
            .Must(o => VariantCombinations.AllDifferent(o!.Select(x => x?.Name))).WithMessage("Each option needs a different name.")
            .When(x => x.Options is not null);
        RuleForEach(x => x.Options).NotNull().SetValidator(new ProductOptionValidator());

        RuleFor(x => x.Variants).NotEmpty();
        RuleForEach(x => x.Variants).NotNull().SetValidator(new VariantRequestValidator());
        RuleFor(x => x.Variants).Custom(VariantCombinations.Validate);
    }
}

public class ProductOptionValidator : AbstractValidator<ProductOption>
{
    public ProductOptionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ProductOption.NameMaxLength).Must(VariantCombinations.IsOneLine)
            .WithMessage(NoControlCharactersMessage);
        RuleFor(x => x.Values).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(v => v.Count <= ProductOption.MaxValues).WithMessage($"An option can have at most {ProductOption.MaxValues} values.")
            .Must(VariantCombinations.AllDifferent).WithMessage("Each value of an option must be different.");
        RuleForEach(x => x.Values).NotEmpty().MaximumLength(ProductOption.ValueMaxLength).Must(VariantCombinations.IsOneLine)
            .WithMessage(NoControlCharactersMessage);
    }

    private const string NoControlCharactersMessage = "Can't contain line breaks or other control characters.";
}

public class VariantRequestValidator : AbstractValidator<VariantRequest>
{
    public VariantRequestValidator()
    {
        RuleFor(x => x.Price).GreaterThan(0).LessThanOrEqualTo(Product.MaxPrice);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.WeightGrams).GreaterThan(0);
    }
}

internal static class ProductDetails
{
    /// <summary>Copies validated <paramref name="details"/> onto the Product, except its Category.</summary>
    public static void ApplyTo(this IProductDetails details, Product product)
    {
        product.Name = details.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(details.Description) ? null : details.Description.Trim();

        var options = (details.Options ?? []).Select(o => new ProductOption(o.Name.Trim(), [.. o.Values.Select(v => v.Trim())])).ToList();
        var variants = details.Variants.Select(v => new VariantDetails(
            VariantCombinations.InOptionSpelling(options, v.OptionValues ?? []), Money.From(v.Price), v.Stock, v.WeightGrams));
        product.SetVariants(options, [.. variants]);
    }
}
