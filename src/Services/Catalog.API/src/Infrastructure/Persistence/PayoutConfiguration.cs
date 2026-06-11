using MicroCommerce.Catalog.Domain.Payouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        builder.ToTable("payouts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => PayoutId.From(value));

        builder.Property(p => p.SentAt)
            .HasColumnName("sent_at");
        builder.HasIndex(p => p.SentAt);

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);

        builder.Property(p => p.Period)
            .HasColumnName("period")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Destination)
            .HasColumnName("destination")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.IsPending)
            .HasColumnName("is_pending");
    }
}
