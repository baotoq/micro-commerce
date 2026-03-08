using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Coupons.Domain.Entities;

public sealed class CouponUsage : Entity<CouponUsageId>
{
    private CouponUsage(CouponUsageId id) : base(id) { }

    public CouponId CouponId { get; private set; }
    public Guid OrderId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public decimal DiscountApplied { get; private set; }
    public DateTimeOffset UsedAt { get; private set; }

    public static CouponUsage Create(
        CouponId couponId,
        Guid orderId,
        string userId,
        decimal discountApplied)
    {
        return new CouponUsage(CouponUsageId.New())
        {
            CouponId = couponId,
            OrderId = orderId,
            UserId = userId,
            DiscountApplied = discountApplied,
            UsedAt = DateTimeOffset.UtcNow
        };
    }
}
