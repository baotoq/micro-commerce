using MicroCommerce.Catalog.Domain.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class AnalyticsSourceConfiguration : IEntityTypeConfiguration<AnalyticsSource>
{
    public void Configure(EntityTypeBuilder<AnalyticsSource> builder)
    {
        builder.ToTable("analytics_sources");

        builder.HasKey(s => s.Id);

        // Synthetic seeded rows use explicit Id values (1..5) — no value generation.
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.Source)
            .HasColumnName("source")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Pct)
            .HasColumnName("pct")
            .HasPrecision(5, 2);

        builder.Property(s => s.Revenue)
            .HasColumnName("revenue")
            .HasPrecision(18, 2);
    }
}
