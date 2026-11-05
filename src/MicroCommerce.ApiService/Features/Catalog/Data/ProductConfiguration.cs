using System.Text.Json;
using MicroCommerce.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace MicroCommerce.ApiService.Features.Catalog.Data;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>The generated full-text column. A shadow property: the domain never sees it (research R6).</summary>
    public const string SearchVector = "SearchVector";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.MerchantId).HasColumnName("merchant_id");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(Product.MaxNameLength);
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(Product.MaxDescriptionLength);
        builder.Property(p => p.CategoryId).HasColumnName("category_id");
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.Version).HasColumnName("version").IsConcurrencyToken();
        builder.Ignore(p => p.DomainEvents);

        // Options are small and always read with the Product: one jsonb column.
        builder.Property(p => p.Options)
            .HasColumnName("options")
            .HasColumnType("jsonb")
            .HasConversion(
                options => JsonSerializer.Serialize(options, Json),
                json => JsonSerializer.Deserialize<List<ProductOption>>(json, Json) ?? new List<ProductOption>(),
                new ValueComparer<IReadOnlyList<ProductOption>>(
                    (a, b) => a!.SequenceEqual(b!),
                    o => o.Aggregate(0, (hash, option) => HashCode.Combine(hash, option.GetHashCode())),
                    o => o.ToList()));

        builder.Navigation(p => p.Variants).HasField("_variants").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(p => p.Variants, variant =>
        {
            variant.ToTable("variants");
            variant.WithOwner().HasForeignKey("product_id");
            variant.HasKey(v => v.Id);
            variant.Property(v => v.Id).HasColumnName("id").ValueGeneratedNever();
            variant.Property(v => v.Position).HasColumnName("position");
            variant.Property(v => v.OptionValues).HasColumnName("option_values");
            variant.Property(v => v.Price).HasColumnName("price");
            variant.Property(v => v.Stock).HasColumnName("stock");
            variant.Property(v => v.WeightGrams).HasColumnName("weight_grams");
        });

        builder.Navigation(p => p.Images).HasField("_images").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(p => p.Images, image =>
        {
            image.ToTable("product_images");
            image.WithOwner().HasForeignKey("product_id");
            image.HasKey(i => i.Id);
            image.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
            image.Property(i => i.Position).HasColumnName("position");
            image.Property(i => i.Format).HasColumnName("format").HasConversion<string>().HasMaxLength(8);
            image.Property(i => i.SizeBytes).HasColumnName("size_bytes");
            image.Property(i => i.UploadedAt).HasColumnName("uploaded_at");
        });

        builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.MerchantId, p.CreatedAt }).IsDescending(false, true).HasDatabaseName("ix_products_merchant_id_created_at");
        builder.HasIndex(p => new { p.Status, p.CreatedAt }).IsDescending(false, true).HasDatabaseName("ix_products_status_created_at");
        builder.HasIndex(p => p.CategoryId).HasDatabaseName("ix_products_category_id");

        builder.Property<NpgsqlTsVector>(SearchVector)
            .HasColumnName("search_vector")
            .IsGeneratedTsVectorColumn("english", [nameof(Product.Name), nameof(Product.Description)]);
        builder.HasIndex(SearchVector).HasMethod("GIN").HasDatabaseName("ix_products_search_vector");
    }
}
