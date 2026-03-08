using MediatR;
using MicroCommerce.ApiService.Features.Coupons.Domain.Entities;
using MicroCommerce.ApiService.Features.Coupons.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;

public sealed class SubmitOrderCommandHandler
    : IRequestHandler<SubmitOrderCommand, Guid>
{
    private readonly OrderingDbContext _context;
    private readonly CouponsDbContext _couponsContext;

    public SubmitOrderCommandHandler(OrderingDbContext context, CouponsDbContext couponsContext)
    {
        _context = context;
        _couponsContext = couponsContext;
    }

    public async Task<Guid> Handle(
        SubmitOrderCommand request,
        CancellationToken cancellationToken)
    {
        ShippingAddress address = new(
            request.ShippingAddress.Name,
            request.ShippingAddress.Email,
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.ZipCode);

        IEnumerable<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items =
            request.Items.Select(i => (i.ProductId, i.ProductName, i.UnitPrice, i.ImageUrl, i.Quantity));

        (string? couponCode, decimal discountAmount, Coupons.Domain.Entities.Coupon? coupon) =
            await ResolveCouponAsync(request, items, cancellationToken);

        Order order = Order.Create(request.BuyerId, request.Email, address, items, couponCode, discountAmount);

        await _context.Orders.AddAsync(order, cancellationToken);

        if (coupon is not null)
        {
            coupon.IncrementUsage();

            CouponUsage usage = CouponUsage.Create(
                coupon.Id,
                order.Id.Value,
                request.BuyerId.ToString(),
                discountAmount);

            await _couponsContext.CouponUsages.AddAsync(usage, cancellationToken);
            await _couponsContext.SaveChangesAsync(cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return order.Id.Value;
    }

    private async Task<(string? couponCode, decimal discountAmount, Coupons.Domain.Entities.Coupon? coupon)> ResolveCouponAsync(
        SubmitOrderCommand request,
        IEnumerable<(Guid productId, string productName, decimal unitPrice, string? imageUrl, int quantity)> items,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CouponCode))
            return (null, 0m, null);

        string upperCode = request.CouponCode.ToUpperInvariant();

        Coupons.Domain.Entities.Coupon? coupon = await _couponsContext.Coupons
            .FirstOrDefaultAsync(c => c.Code == upperCode, cancellationToken);

        if (coupon is null)
            return (null, 0m, null);

        decimal subtotal = items.Sum(i => i.unitPrice * i.quantity);

        int userUsageCount = 0;
        if (coupon.UsagePerUser.HasValue)
        {
            string userId = request.BuyerId.ToString();
            userUsageCount = await _couponsContext.CouponUsages
                .CountAsync(u => u.CouponId == coupon.Id && u.UserId == userId, cancellationToken);
        }

        (bool isValid, decimal discountAmount, string? _) =
            coupon.Validate(subtotal, DateTimeOffset.UtcNow, userUsageCount);

        if (!isValid)
            return (null, 0m, null);

        return (coupon.Code, discountAmount, coupon);
    }
}
