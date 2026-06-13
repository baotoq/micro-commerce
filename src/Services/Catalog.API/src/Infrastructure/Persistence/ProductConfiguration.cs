using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => ProductId.From(value));

        builder.Property(p => p.Sku)
            .HasConversion(sku => sku.Value, value => Sku.From(value))
            .HasMaxLength(50)
            .IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Category).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(p => p.Tags)
            .HasColumnName("tags")
            .HasColumnType("text[]")
            .IsRequired()
            .HasDefaultValueSql("'{}'::text[]");

        builder.Property(p => p.Weight)
            .HasColumnName("weight")
            .HasColumnType("numeric(6,2)")
            .IsRequired();

        builder.Property(p => p.Origin)
            .HasColumnName("origin")
            .HasColumnType("varchar(60)")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(p => p.PhotoUrls)
            .HasColumnName("photo_urls")
            .HasColumnType("text[]")
            .IsRequired()
            .HasDefaultValueSql("'{}'::text[]");

        // GIN index on tags for fast array-containment queries (PRD §2).
        builder.HasIndex(p => p.Tags).HasMethod("gin");

        // Optimistic concurrency token (review finding 2): bind Postgres' existing `xmin` system
        // column as the concurrency token so concurrent storefront checkouts that decrement the
        // same product can't lost-update each other into negative inventory.
        // PlaceStorefrontOrderHandler catches the resulting DbUpdateConcurrencyException, reloads,
        // and retries. (EF Core 10 dropped UseXminAsConcurrencyToken; this is the Npgsql idiom — a
        // uint shadow prop typed `xid` + OnAddOrUpdate + concurrency token binds to the system
        // column and generates NO migration AddColumn, since xmin already exists on every table.)
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
