namespace MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCoupons;

public sealed record CouponDto(
    Guid Id,
    string Code,
    string Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int? UsagePerUser,
    int TimesUsed,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil,
    bool IsActive,
    List<Guid> ApplicableProductIds,
    List<Guid> ApplicableCategoryIds,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CouponListDto(
    List<CouponDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
