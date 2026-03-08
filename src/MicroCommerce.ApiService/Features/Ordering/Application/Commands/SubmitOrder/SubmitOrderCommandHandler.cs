using FluentResults;
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
    private readonly OrderingDbContext _orderingContext;
    private readonly CouponsDbContext _couponsContext;

    public SubmitOrderCommandHandler(OrderingDbContext orderingContext, CouponsDbContext couponsContext)
    {
        _orderingContext = orderingContext;
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

        decimal subtotal = request.Items.Sum(i => i.UnitPrice * i.Quantity);
        string? resolvedCouponCode = null;
        decimal discountAmount = 0;
        Coupon? coupon = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            string upperCode = request.CouponCode.ToUpperInvariant();
            coupon = await _couponsContext.Coupons
                .FirstOrDefaultAsync(c => c.Code == upperCode, cancellationToken);

            if (coupon is null)
                throw new InvalidOperationException($"Coupon '{request.CouponCode}' not found.");

            int userUsageCount = 0;
            if (coupon.UsagePerUser.HasValue)
            {
                string userId = request.BuyerId.ToString();
                userUsageCount = await _couponsContext.CouponUsages
                    .CountAsync(u => u.CouponId == coupon.Id && u.UserId == userId, cancellationToken);
            }

            (bool isValid, decimal calculatedDiscount, string? errorMessage) =
                coupon.Validate(subtotal, DateTimeOffset.UtcNow, userUsageCount);

            if (!isValid)
                throw new InvalidOperationException(errorMessage ?? "Coupon is not valid.");

            resolvedCouponCode = coupon.Code;
            discountAmount = calculatedDiscount;
        }

        Order order = Order.Create(request.BuyerId, request.Email, address, items, resolvedCouponCode, discountAmount);

        await _orderingContext.Orders.AddAsync(order, cancellationToken);

        if (coupon is not null)
        {
            coupon.IncrementUsage();
            CouponUsage usage = CouponUsage.Create(coupon.Id, order.Id.Value, request.BuyerId.ToString(), discountAmount);
            await _couponsContext.CouponUsages.AddAsync(usage, cancellationToken);
            await _couponsContext.SaveChangesAsync(cancellationToken);
        }

        await _orderingContext.SaveChangesAsync(cancellationToken);

        return order.Id.Value;
    }
}
