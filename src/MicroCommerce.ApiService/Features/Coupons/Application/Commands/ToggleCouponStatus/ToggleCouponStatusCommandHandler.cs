using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Commands.ToggleCouponStatus;

public sealed class ToggleCouponStatusCommandHandler : IRequestHandler<ToggleCouponStatusCommand>
{
    private readonly CouponsDbContext _context;

    public ToggleCouponStatusCommandHandler(CouponsDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ToggleCouponStatusCommand request, CancellationToken cancellationToken)
    {
        CouponId couponId = CouponId.From(request.Id);

        Domain.Entities.Coupon coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken)
            ?? throw new NotFoundException($"Coupon with ID {request.Id} not found.");

        coupon.SetActiveStatus(request.IsActive);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
