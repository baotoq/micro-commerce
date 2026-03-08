using MediatR;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Queries.ValidateCoupon;

public sealed class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, ValidateCouponResult>
{
    private readonly CouponsDbContext _context;

    public ValidateCouponQueryHandler(CouponsDbContext context)
    {
        _context = context;
    }

    public async Task<ValidateCouponResult> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        string upperCode = request.Code.ToUpperInvariant();

        Domain.Entities.Coupon? coupon = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == upperCode, cancellationToken);

        if (coupon is null)
            return new ValidateCouponResult(false, 0, "Coupon not found.");

        int userUsageCount = 0;
        if (!string.IsNullOrEmpty(request.UserId) && coupon.UsagePerUser.HasValue)
        {
            userUsageCount = await _context.CouponUsages
                .AsNoTracking()
                .CountAsync(u => u.CouponId == coupon.Id && u.UserId == request.UserId, cancellationToken);
        }

        (bool isValid, decimal discountAmount, string? errorMessage) =
            coupon.Validate(request.Subtotal, DateTimeOffset.UtcNow, userUsageCount);

        return new ValidateCouponResult(isValid, discountAmount, errorMessage);
    }
}
