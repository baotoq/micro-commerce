using MediatR;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Commands.UpdateCoupon;

public sealed record UpdateCouponCommand(
    Guid Id,
    string Description,
    string DiscountType,
    decimal DiscountValue,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil = null,
    decimal? MinOrderAmount = null,
    decimal? MaxDiscountAmount = null,
    int? UsageLimit = null,
    int? UsagePerUser = null,
    List<Guid>? ApplicableProductIds = null,
    List<Guid>? ApplicableCategoryIds = null) : IRequest;
