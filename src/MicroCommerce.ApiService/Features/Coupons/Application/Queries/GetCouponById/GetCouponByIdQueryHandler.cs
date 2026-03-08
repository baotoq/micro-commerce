using MediatR;
using MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCoupons;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCouponById;

public sealed class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, CouponDto?>
{
    private readonly CouponsDbContext _context;

    public GetCouponByIdQueryHandler(CouponsDbContext context)
    {
        _context = context;
    }

    public async Task<CouponDto?> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        CouponId couponId = CouponId.From(request.Id);

        return await _context.Coupons
            .AsNoTracking()
            .Where(c => c.Id == couponId)
            .Select(c => new CouponDto(
                c.Id.Value,
                c.Code,
                c.Description,
                c.DiscountType.Name,
                c.DiscountValue,
                c.MinOrderAmount,
                c.MaxDiscountAmount,
                c.UsageLimit,
                c.UsagePerUser,
                c.TimesUsed,
                c.ValidFrom,
                c.ValidUntil,
                c.IsActive,
                c.ApplicableProductIds,
                c.ApplicableCategoryIds,
                c.CreatedAt,
                c.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
