using MicroCommerce.Catalog.Domain.Payouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("ledger_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => LedgerEntryId.From(value));

        builder.Property(e => e.OccurredAt)
            .HasColumnName("occurred_at");
        builder.HasIndex(e => e.OccurredAt);

        builder.Property(e => e.Label)
            .HasColumnName("label")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(e => e.Subject)
            .HasColumnName("subject")
            .HasMaxLength(120);

        builder.Property(e => e.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        builder.Property(e => e.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Display hint referencing an order; not a real foreign key.
        builder.Property(e => e.OrderNumber)
            .HasColumnName("order_number");
    }
}
