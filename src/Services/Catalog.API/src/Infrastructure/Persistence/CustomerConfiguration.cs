using MicroCommerce.Catalog.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => CustomerId.From(value));

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasConversion(e => e.Value, v => Email.From(v))
            .HasMaxLength(200)
            .IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();

        builder.Property(c => c.City)
            .HasColumnName("city")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Tags: collection of simple strings backed by the private _tags field.
        //
        // DEVIATION from plan 2.2 (which spec'd `OwnsMany(c => c.Tags, ...)` into a separate
        // `customer_tags` table). EF Core 10 rejects `OwnsMany<string>` — an owned type must be
        // a non-primitive reference type ("The specified type 'System.String' must be a
        // non-interface reference type to be used as an entity type."). PrimitiveCollection is
        // the supported way to map a List<string> without changing the domain; on Npgsql it maps
        // to a `tags text[]` column (same shape ProductConfiguration uses for Product.Tags) and it
        // round-trips correctly under the in-memory provider used by Application.Tests.
        builder.PrimitiveCollection<IReadOnlyList<string>>("Tags")
            .HasField("_tags")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("tags")
            .HasColumnType("text[]")
            .IsRequired()
            .HasDefaultValueSql("'{}'::text[]");
    }
}
