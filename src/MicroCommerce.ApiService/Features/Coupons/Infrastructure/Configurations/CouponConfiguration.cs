using MicroCommerce.ApiService.Features.Coupons.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Coupons.Infrastructure.Configurations;

public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.DiscountValue)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(c => c.MinOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.MaxDiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.TimesUsed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.ValidFrom)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.ApplicableProductIds)
            .HasColumnType("jsonb");

        builder.Property(c => c.ApplicableCategoryIds)
            .HasColumnType("jsonb");

        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.ValidFrom);
        builder.HasIndex(c => c.ValidUntil);

        builder.HasMany<CouponUsage>()
            .WithOne()
            .HasForeignKey(u => u.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
