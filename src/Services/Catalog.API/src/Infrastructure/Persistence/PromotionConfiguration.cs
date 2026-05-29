using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions", t => t.HasCheckConstraint(
            "ck_promotions_kind_value_exclusive",
            "(kind = 'Percentage' AND percent_value IS NOT NULL AND fixed_amount IS NULL) OR " +
            "(kind = 'FixedAmount' AND fixed_amount IS NOT NULL AND percent_value IS NULL)"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => PromotionId.From(value));

        builder.Property(p => p.Code)
            .HasConversion(code => code.Value, value => PromotionCode.From(value))
            .HasColumnName("code")
            .HasMaxLength(40)
            .IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.PercentValue)
            .HasColumnName("percent_value");

        builder.Property(p => p.FixedAmount)
            .HasColumnName("fixed_amount")
            .HasPrecision(18, 2);

        builder.Property(p => p.MinOrderAmount)
            .HasColumnName("min_order_amount")
            .HasPrecision(18, 2);

        builder.Property(p => p.StartsAt)
            .HasColumnName("starts_at");

        builder.Property(p => p.EndsAt)
            .HasColumnName("ends_at");

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();
    }
}
