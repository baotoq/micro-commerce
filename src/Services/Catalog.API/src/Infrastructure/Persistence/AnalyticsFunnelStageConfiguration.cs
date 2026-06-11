using MicroCommerce.Catalog.Domain.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class AnalyticsFunnelStageConfiguration : IEntityTypeConfiguration<AnalyticsFunnelStage>
{
    public void Configure(EntityTypeBuilder<AnalyticsFunnelStage> builder)
    {
        builder.ToTable("analytics_funnel");

        builder.HasKey(f => f.Id);

        // Synthetic seeded rows use explicit Id values (1..5) — no value generation.
        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(f => f.Label)
            .HasColumnName("label")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.Count)
            .HasColumnName("count");

        builder.Property(f => f.Rate)
            .HasColumnName("rate")
            .HasPrecision(5, 4);
    }
}
