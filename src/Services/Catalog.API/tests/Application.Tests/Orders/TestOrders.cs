using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Application.Tests.Orders;

internal static class TestOrders
{
    public static OrderLine Line(
        string sku = "MC-VS-001",
        string productName = "Persimmon vase",
        string tone = "clay",
        int qty = 1,
        decimal unitPrice = 86.00m,
        FulfillmentStatus fulfillment = FulfillmentStatus.Awaiting,
        string? tracking = null,
        string? restockNote = null) =>
        new(sku, productName, tone, qty, unitPrice, fulfillment, tracking, restockNote);

    /// <summary>
    /// Build a New order. Defaults mirror the canonical #1042 single-line shape unless overridden.
    /// </summary>
    public static Order CreateNew(
        int number = 1042,
        DateTimeOffset? placedAt = null,
        string customerName = "Sasha Leblanc",
        string customerEmail = "sasha.l@gmail.com",
        string cityState = "San Francisco, CA",
        string shipLine1 = "820 Sutter St · #4B",
        string shipLine2 = "San Francisco, CA 94109",
        bool billSameAsShip = true,
        string itemsSummary = "Persimmon vase",
        string shippingMethod = "USPS Priority",
        decimal shippingPaid = 0m,
        decimal tax = 0m,
        decimal feePct = 4m,
        string paymentBrand = "Visa",
        string paymentLastFour = "4421",
        string? labelCarrier = "USPS",
        decimal labelCost = 0m,
        string? labelWeightLabel = "1lb 4oz",
        string internalNote = "",
        IReadOnlyList<OrderLine>? lines = null) =>
        new(
            number,
            placedAt ?? new DateTimeOffset(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7)),
            customerName,
            customerEmail,
            cityState,
            shipLine1,
            shipLine2,
            billSameAsShip,
            itemsSummary,
            shippingMethod,
            shippingPaid,
            tax,
            feePct,
            paymentBrand,
            paymentLastFour,
            labelCarrier,
            labelCost,
            labelWeightLabel,
            internalNote,
            lines ?? [Line()]);

    public static Order CreatePacked(int number = 1039, string customerEmail = "kai.z@gmail.com", IReadOnlyList<OrderLine>? lines = null)
    {
        var order = CreateNew(number: number, customerEmail: customerEmail, lines: lines);
        order.Pack();
        return order;
    }

    public static Order CreateShipped(int number = 1038, string customerEmail = "jordan.e@gmail.com", IReadOnlyList<OrderLine>? lines = null)
    {
        var order = CreatePacked(number: number, customerEmail: customerEmail, lines: lines);
        order.Ship("USPS", "USPS · tracking");
        return order;
    }

    public static Order CreateDelivered(int number = 1035, string customerEmail = "priya.s@gmail.com", IReadOnlyList<OrderLine>? lines = null)
    {
        var order = CreateShipped(number: number, customerEmail: customerEmail, lines: lines);
        order.Deliver();
        return order;
    }

    public static Order CreateCancelled(int number = 1033, string customerEmail = "yuki.t@gmail.com", IReadOnlyList<OrderLine>? lines = null)
    {
        var order = CreateNew(number: number, customerEmail: customerEmail, lines: lines);
        order.Cancel();
        return order;
    }

    public static Order CreateRefundRequested(int number = 1036, string customerEmail = "marco.r@gmail.com", IReadOnlyList<OrderLine>? lines = null)
    {
        var order = CreateShipped(number: number, customerEmail: customerEmail, lines: lines);
        order.RequestRefund(new RefundDraft(
            "Item arrived chipped",
            "Damage",
            30.00m,
            [new RefundDraftItem("MC-VS-001", 1, 86.00m, false, null)]));
        return order;
    }
}
