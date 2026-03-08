using MediatR;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Queries.ValidateCoupon;

public sealed record ValidateCouponQuery(
    string Code,
    decimal Subtotal,
    string? UserId = null) : IRequest<ValidateCouponResult>;

public sealed record ValidateCouponResult(
    bool IsValid,
    decimal DiscountAmount,
    string? ErrorMessage);
