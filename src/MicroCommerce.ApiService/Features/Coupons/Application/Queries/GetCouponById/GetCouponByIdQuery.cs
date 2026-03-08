using MediatR;
using MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCoupons;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCouponById;

public sealed record GetCouponByIdQuery(Guid Id) : IRequest<CouponDto?>;
