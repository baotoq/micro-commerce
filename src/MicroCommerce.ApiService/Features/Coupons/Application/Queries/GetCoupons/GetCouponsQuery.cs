using MediatR;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCoupons;

public sealed record GetCouponsQuery(
    int Page = 1,
    int PageSize = 20,
    bool? IsActive = null,
    string? Search = null) : IRequest<CouponListDto>;
