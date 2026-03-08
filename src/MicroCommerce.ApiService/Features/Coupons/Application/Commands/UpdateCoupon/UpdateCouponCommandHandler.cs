using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Commands.UpdateCoupon;

public sealed class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand>
{
    private readonly CouponsDbContext _context;

    public UpdateCouponCommandHandler(CouponsDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        CouponId couponId = CouponId.From(request.Id);

        Domain.Entities.Coupon coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken)
            ?? throw new NotFoundException($"Coupon with ID {request.Id} not found.");

        DiscountType discountType = DiscountType.FromName(request.DiscountType, ignoreCase: true);

        coupon.Update(
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

        await _context.SaveChangesAsync(cancellationToken);
    }
}
