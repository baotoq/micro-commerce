using MediatR;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Queries.GetCoupons;

public sealed class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, CouponListDto>
{
    private readonly CouponsDbContext _context;

    public GetCouponsQueryHandler(CouponsDbContext context)
    {
        _context = context;
    }

    public async Task<CouponListDto> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Coupon> query = _context.Coupons.AsNoTracking();

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.ToUpperInvariant();
            query = query.Where(c => c.Code.Contains(search) || c.Description.ToUpper().Contains(request.Search.ToUpper()));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<CouponDto> items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToListAsync(cancellationToken);

        return new CouponListDto(items, totalCount, request.Page, request.PageSize);
    }
}
