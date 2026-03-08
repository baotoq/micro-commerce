using MediatR;
using MicroCommerce.ApiService.Features.Coupons.Domain.Entities;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Commands.CreateCoupon;

public sealed class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Guid>
{
    private readonly CouponsDbContext _context;

    public CreateCouponCommandHandler(CouponsDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        string upperCode = request.Code.ToUpperInvariant();

        bool exists = await _context.Coupons
            .AnyAsync(c => c.Code == upperCode, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"A coupon with code '{upperCode}' already exists.");

        DiscountType discountType = DiscountType.FromName(request.DiscountType, ignoreCase: true);

        Coupon coupon = Coupon.Create(
            upperCode,
            request.Description,
            discountType,
            request.DiscountValue,
            request.ValidFrom,
            request.ValidUntil,
            request.MinOrderAmount,
            request.MaxDiscountAmount,
            request.UsageLimit,
            request.UsagePerUser,
            request.ApplicableProductIds,
            request.ApplicableCategoryIds);

        await _context.Coupons.AddAsync(coupon, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return coupon.Id.Value;
    }
}
