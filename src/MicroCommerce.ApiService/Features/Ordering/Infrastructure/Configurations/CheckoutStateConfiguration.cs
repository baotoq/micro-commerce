using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Ordering.Infrastructure.Configurations;

public sealed class CheckoutStateConfiguration : IEntityTypeConfiguration<CheckoutState>
{
    public void Configure(EntityTypeBuilder<CheckoutState> builder)
    {
        builder.ToTable("CheckoutSagas");

        builder.HasKey(x => x.CorrelationId);

        builder.Property(x => x.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.OrderId);

        builder.Property(x => x.BuyerId);

        builder.Property(x => x.BuyerEmail)
            .HasMaxLength(256);

        builder.Property(x => x.SubmittedAt);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(1024);

        builder.Property(x => x.ReservationIdsJson)
            .HasMaxLength(4096);

        // PostgreSQL xmin optimistic concurrency
        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}
