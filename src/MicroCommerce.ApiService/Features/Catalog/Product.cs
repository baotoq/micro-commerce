using System.Text.Json.Serialization;
using FluentValidation;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// A listing a Merchant offers, in exactly one leaf Category. It is never bought directly; its Variants are.
/// Until Products have options, each has exactly one default Variant.
/// </summary>
public class Product
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 5000;

    public ProductId Id { get; init; } = ProductId.New();

    /// <summary>The Merchant that owns the Product; every Merchant Centre query is scoped by it (ADR-0001).</summary>
    public required MerchantId MerchantId { get; init; }

    public required string Name { get; set; }
    public string? Description { get; set; }
    public required CategoryId CategoryId { get; set; }

    /// <summary>New Products start as Draft; edits keep the current status.</summary>
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;

    public List<Variant> Variants { get; } = [];
    public DateTimeOffset CreatedAt { get; init; } = DatabaseClock.UtcNow;

    public void Publish() => Status = ProductStatus.Published;

    public void Unpublish() => Status = ProductStatus.Draft;

    /// <summary>
    /// Sets the price, stock and weight of the single default Variant, creating it on first use. An existing
    /// default Variant keeps its ID.
    /// </summary>
    public void SetDefaultVariant(Money price, int stock, int weightGrams)
    {
        if (Variants.Count > 1)
            throw new InvalidOperationException($"Product {Id} has {Variants.Count} Variants, not a single default one.");

        if (Variants.Count == 0) Variants.Add(new Variant { Price = price, Stock = stock, WeightGrams = weightGrams });
        else
        {
            var variant = Variants[0];
            variant.Price = price;
            variant.Stock = stock;
            variant.WeightGrams = weightGrams;
        }
    }
}

/// <summary>
/// <c>Draft</c> → <c>Published</c> → <c>Draft</c>, moved by the Merchant. Buyers only ever see Published Products.
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
    IReadOnlyList<VariantResponse> Variants,
    DateTimeOffset CreatedAt);

public record VariantResponse(VariantId Id, Money Price, int Stock, int WeightGrams);

/// <summary>A row of the Merchant's Product list: its price range and total stock across Variants.</summary>
public record ProductSummaryResponse(
    ProductId Id,
    string Name,
    CategoryId CategoryId,
    ProductStatus Status,
    int VariantCount,
    Money MinPrice,
    Money MaxPrice,
    int TotalStock,
    DateTimeOffset CreatedAt);

/// <summary>What a Merchant gives when creating or editing a Product without options.</summary>
public interface IProductDetails
{
    string Name { get; }
    string? Description { get; }
    CategoryId? CategoryId { get; }

    /// <summary>The default Variant's price, in whole VND.</summary>
    long Price { get; }

    int Stock { get; }
    int WeightGrams { get; }
}

public class ProductDetailsValidator : AbstractValidator<IProductDetails>
{
    public ProductDetailsValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(x => x.Description).MaximumLength(Product.DescriptionMaxLength);
        RuleFor(x => x.CategoryId).NotNull();
        RuleFor(x => x.Price).GreaterThan(0);
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
        product.SetDefaultVariant(Money.From(details.Price), details.Stock, details.WeightGrams);
    }
}
