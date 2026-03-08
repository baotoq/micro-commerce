using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Coupons.Domain.Entities;

public sealed class Coupon : AuditableAggregateRoot<CouponId>
{
    private Coupon(CouponId id) : base(id) { }

    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DiscountType DiscountType { get; private set; } = DiscountType.FixedAmount;
    public decimal DiscountValue { get; private set; }
    public decimal? MinOrderAmount { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int? UsagePerUser { get; private set; }
    public int TimesUsed { get; private set; }
    public DateTimeOffset ValidFrom { get; private set; }
    public DateTimeOffset? ValidUntil { get; private set; }
    public bool IsActive { get; private set; }
    public List<Guid> ApplicableProductIds { get; private set; } = [];
    public List<Guid> ApplicableCategoryIds { get; private set; } = [];

    public static Coupon Create(
        string code,
        string description,
        DiscountType discountType,
        decimal discountValue,
        DateTimeOffset validFrom,
        DateTimeOffset? validUntil = null,
        decimal? minOrderAmount = null,
        decimal? maxDiscountAmount = null,
        int? usageLimit = null,
        int? usagePerUser = null,
        List<Guid>? applicableProductIds = null,
        List<Guid>? applicableCategoryIds = null)
    {
        return new Coupon(CouponId.New())
        {
            Code = code.ToUpperInvariant(),
            Description = description,
            DiscountType = discountType,
            DiscountValue = discountValue,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            MinOrderAmount = minOrderAmount,
            MaxDiscountAmount = maxDiscountAmount,
            UsageLimit = usageLimit,
            UsagePerUser = usagePerUser,
            IsActive = true,
            TimesUsed = 0,
            ApplicableProductIds = applicableProductIds ?? [],
            ApplicableCategoryIds = applicableCategoryIds ?? []
        };
    }

    public void Update(
        string description,
        DiscountType discountType,
        decimal discountValue,
        DateTimeOffset validFrom,
        DateTimeOffset? validUntil,
        decimal? minOrderAmount,
        decimal? maxDiscountAmount,
        int? usageLimit,
        int? usagePerUser,
        List<Guid>? applicableProductIds,
        List<Guid>? applicableCategoryIds)
    {
        Description = description;
        DiscountType = discountType;
        DiscountValue = discountValue;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        MinOrderAmount = minOrderAmount;
        MaxDiscountAmount = maxDiscountAmount;
        UsageLimit = usageLimit;
        UsagePerUser = usagePerUser;
        ApplicableProductIds = applicableProductIds ?? [];
        ApplicableCategoryIds = applicableCategoryIds ?? [];
    }

    public void SetActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }

    public void IncrementUsage()
    {
        TimesUsed++;
    }

    public (bool IsValid, decimal DiscountAmount, string? ErrorMessage) Validate(
        decimal subtotal,
        DateTimeOffset now,
        int userUsageCount)
    {
        if (!IsActive)
            return (false, 0, "Coupon is not active.");

        if (now < ValidFrom)
            return (false, 0, "Coupon is not yet valid.");

        if (ValidUntil.HasValue && now > ValidUntil.Value)
            return (false, 0, "Coupon has expired.");

        if (UsageLimit.HasValue && TimesUsed >= UsageLimit.Value)
            return (false, 0, "Coupon usage limit has been reached.");

        if (UsagePerUser.HasValue && userUsageCount >= UsagePerUser.Value)
            return (false, 0, "You have reached the usage limit for this coupon.");

        if (MinOrderAmount.HasValue && subtotal < MinOrderAmount.Value)
            return (false, 0, $"Minimum order amount of {MinOrderAmount.Value:C} is required.");

        decimal discountAmount = DiscountType.Calculate(subtotal, DiscountValue, MaxDiscountAmount);
        return (true, discountAmount, null);
    }
}
