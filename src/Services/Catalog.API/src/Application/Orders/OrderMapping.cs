using MicroCommerce.Catalog.Application.Orders.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Customers;
using MicroCommerce.Catalog.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Orders;

internal static class OrderMapping
{
    /// <summary>
    /// Builds the full detail DTO including the customer snapshot enriched with lifetime
    /// stats (count/spend grouped by email, excluding Cancelled) and Customer tags.
    /// Used by mutating handlers after a status change so the returned DTO is canonical.
    /// </summary>
    public static async Task<OrderDetailDto> ToDetailDtoWithCustomerAsync(AppDbContext db, Order order, CancellationToken ct)
    {
        var email = order.CustomerEmail;
        var customerOrders = await db.Orders.AsNoTracking()
            .Where(o => o.CustomerEmail == email && o.Status != OrderStatus.Cancelled)
            .ToListAsync(ct);
        var lifetimeOrderCount = customerOrders.Count;
        var lifetimeSpend = customerOrders.Sum(o => o.Paid);

        var customerEmail = Email.From(email);
        var customer = await db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == customerEmail, ct);
        IReadOnlyList<string> tags = customer?.Tags.ToList() ?? [];

        var customerDto = new OrderCustomerDto(
            order.CustomerName,
            order.CustomerEmail,
            order.ShipLine1,
            order.ShipLine2,
            order.BillSameAsShip,
            lifetimeOrderCount,
            lifetimeSpend,
            tags);

        return ToDetailDto(order, customerDto);
    }

    public static string StatusToWireKey(OrderStatus s) => s switch
    {
        OrderStatus.New => "new",
        OrderStatus.Packed => "packed",
        OrderStatus.Shipped => "shipped",
        OrderStatus.Delivered => "delivered",
        OrderStatus.RefundRequested => "refund-requested",
        OrderStatus.Cancelled => "cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, "Unknown order status."),
    };

    public static string FulfillmentToWireKey(FulfillmentStatus s) => s switch
    {
        FulfillmentStatus.Awaiting => "awaiting",
        FulfillmentStatus.Shipped => "shipped",
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, "Unknown fulfillment status."),
    };

    public static FulfillmentStatus ParseFulfillment(string s) => s.ToLowerInvariant() switch
    {
        "awaiting" => FulfillmentStatus.Awaiting,
        "shipped" => FulfillmentStatus.Shipped,
        _ => throw new ArgumentException($"Invalid fulfillment status: {s}"),
    };

    public static OrderInboxRowDto ToInboxRowDto(Order o) =>
        new(
            o.Number,
            o.PlacedAt,
            o.CustomerName,
            o.CityState,
            o.ItemsSummary,
            o.Lines.Sum(l => l.Qty),
            o.Paid,
            o.ShippingMethod,
            StatusToWireKey(o.Status),
            o.Starred);

    public static OrderDetailDto ToDetailDto(Order o, OrderCustomerDto customer)
    {
        var lineCount = o.Lines.Count;
        var lines = o.Lines
            .Select((l, i) => new OrderLineDto(
                i + 1,
                lineCount,
                FulfillmentToWireKey(l.FulfillmentStatus),
                l.Sku,
                l.ProductName,
                l.Tone,
                l.Qty,
                l.UnitPrice,
                l.Tracking,
                l.RestockNote))
            .ToList();

        RefundDraftDto? refund = o.Refund is null
            ? null
            : new RefundDraftDto(
                o.Refund.Items.Sum(i => i.Amount),
                o.Refund.Items.Count,
                o.Refund.Items
                    .Select(i => new RefundDraftItemDto(i.Sku, i.Qty, i.Amount, i.Selected, i.PartialAmount))
                    .ToList(),
                o.Refund.Reason,
                o.Refund.RestockChoice,
                o.Refund.Total,
                o.PaymentLastFour);

        var summary = new OrderSummaryDto(
            o.Subtotal,
            o.Lines.Sum(l => l.Qty),
            o.ShippingPaid,
            o.Tax,
            o.Paid,
            o.FeePct,
            o.Fee,
            o.LabelCarrier,
            o.LabelCost,
            o.Net,
            o.DiscountCode,
            o.DiscountAmount);

        var timeline = o.Timeline
            .Select(t => new OrderTimelineDto(t.Icon, t.Title, t.Sub, t.OccurredAt, t.Tone, t.Highlight))
            .ToList();

        var outstanding = o.Lines
            .FirstOrDefault(l => l.FulfillmentStatus == FulfillmentStatus.Awaiting)?.ProductName;

        return new OrderDetailDto(
            o.Number,
            StatusToWireKey(o.Status),
            o.PlacedAt,
            lines,
            refund,
            customer,
            summary,
            o.InternalNote,
            timeline,
            outstanding);
    }
}
