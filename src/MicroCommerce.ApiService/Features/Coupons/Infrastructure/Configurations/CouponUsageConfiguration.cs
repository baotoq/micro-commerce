using MicroCommerce.ApiService.Features.Coupons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Coupons.Infrastructure.Configurations;

public sealed class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.OrderId)
            .IsRequired();

        builder.Property(u => u.UserId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.DiscountApplied)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(u => u.UsedAt)
            .IsRequired();

        builder.HasIndex(u => u.CouponId);
        builder.HasIndex(u => new { u.CouponId, u.UserId });
        builder.HasIndex(u => u.OrderId).IsUnique();
    }
}
