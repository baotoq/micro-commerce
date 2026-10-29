using MicroCommerce.ApiService.Features.Ordering.Data;
using MicroCommerce.ApiService.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Ordering;

public static class OrderResponses
{
    public static async Task<IReadOnlyList<OrderResponse>> LoadAsync(
        OrderingDbContext db,
        IReadOnlyCollection<CheckoutId> checkoutIds,
        MerchantId? merchantId,
        CancellationToken ct)
    {
        if (checkoutIds.Count == 0) return [];

        var ordersQuery = db.Orders.AsNoTracking().Where(order => checkoutIds.Contains(order.CheckoutId));
        if (merchantId is { } id) ordersQuery = ordersQuery.Where(order => order.MerchantId == id);
        var orders = await ordersQuery
            .Include(order => order.Checkout)
            .OrderByDescending(order => order.CreatedAt)
            .ThenBy(order => order.Id)
            .ToListAsync(ct);
        if (orders.Count == 0) return [];

        var ids = orders.Select(order => order.CheckoutId).Distinct().ToArray();
        var groups = await db.CheckoutMerchants.AsNoTracking()
            .Where(group => ids.Contains(group.CheckoutId))
            .ToDictionaryAsync(group => (group.CheckoutId, group.MerchantId), ct);
        var merchantIds = orders.Select(order => order.MerchantId).Distinct().ToArray();
        var lines = await db.OrderLines.AsNoTracking()
            .Where(line => ids.Contains(line.CheckoutId) && merchantIds.Contains(line.MerchantId))
            .ToListAsync(ct);

        return orders.Select(order =>
        {
            var group = groups[(order.CheckoutId, order.MerchantId)];
            var orderLines = lines
                .Where(line => line.CheckoutId == order.CheckoutId && line.MerchantId == order.MerchantId)
                .Select(line => line.ToOrderLineResponse())
                .ToArray();
            return new OrderResponse(
                order.Id,
                order.CheckoutId,
                order.MerchantId,
                order.Status,
                order.CreatedAt,
                group.Subtotal,
                group.ShippingFee,
                order.Checkout.DeliveryAddress,
                orderLines);
        }).ToArray();
    }
}
