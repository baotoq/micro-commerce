using MicroCommerce.Catalog.Domain.Orders;

namespace MicroCommerce.Catalog.Domain.Tests.Orders;

public class OrderTests
{
    private static readonly DateTimeOffset PlacedAt =
        new(2026, 4, 8, 14, 14, 0, TimeSpan.FromHours(-7));

    private static OrderLine Line(
        string sku = "MC-VS-001",
        string productName = "Persimmon vase",
        string tone = "clay",
        int qty = 1,
        decimal unitPrice = 86.00m,
        FulfillmentStatus fulfillmentStatus = FulfillmentStatus.Awaiting,
        string? tracking = null,
        string? restockNote = null) =>
        new(sku, productName, tone, qty, unitPrice, fulfillmentStatus, tracking, restockNote);

    private static Order Create(
        int number = 1042,
        IReadOnlyList<OrderLine>? lines = null,
        decimal shippingPaid = 0m,
        decimal tax = 0m,
        decimal feePct = 4m,
        decimal labelCost = 9.84m,
        string customerName = "Sasha Leblanc",
        string customerEmail = "sasha.l@gmail.com",
        string cityState = "San Francisco, CA")
    {
        lines ??= new List<OrderLine> { Line() };
        return new Order(
            number,
            PlacedAt,
            customerName,
            customerEmail,
            cityState,
            shipLine1: "820 Sutter St · #4B",
            shipLine2: "San Francisco, CA 94109",
            billSameAsShip: true,
            itemsSummary: "Persimmon vase, Ash budstem",
            shippingMethod: "USPS Priority",
            shippingPaid: shippingPaid,
            tax: tax,
            feePct: feePct,
            paymentBrand: "Visa",
            paymentLastFour: "4421",
            labelCarrier: "USPS",
            labelCost: labelCost,
            labelWeightLabel: "1lb 4oz",
            internalNote: "Held until budstem restocks Tue.",
            lines: lines);
    }

    // The #1042 two-line fixture used for the money math assertion.
    private static Order Create1042()
    {
        var lines = new List<OrderLine>
        {
            Line("MC-VS-001", "Persimmon vase", "clay", 1, 86.00m, FulfillmentStatus.Shipped,
                "USPS · 9405 5036 9930 0124 2317", null),
            Line("MC-AB-002", "Ash budstem", "rust", 1, 66.00m, FulfillmentStatus.Awaiting,
                null, "back in stock Tue"),
        };
        return Create(lines: lines, shippingPaid: 0m, tax: 0m, feePct: 4m, labelCost: 9.84m);
    }

    [Fact]
    public void Order_ValidLines_CreatesAsNew()
    {
        var order = Create();
        Assert.NotEqual(default, order.Id.Value);
        Assert.Equal(1042, order.Number);
        Assert.Equal(OrderStatus.New, order.Status);
        Assert.False(order.Starred);
        Assert.Single(order.Lines);
        Assert.Empty(order.Timeline);
        Assert.Null(order.Refund);
    }

    [Fact]
    public void Order_NoLines_Throws()
    {
        Assert.Throws<ArgumentException>(() => Create(lines: new List<OrderLine>()));
    }

    [Fact]
    public void Order_NegativeMoney_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(shippingPaid: -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(tax: -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(labelCost: -1m));
    }

    [Fact]
    public void Pack_FromNew_TransitionsToPacked()
    {
        var order = Create();
        order.Pack();
        Assert.Equal(OrderStatus.Packed, order.Status);
    }

    [Fact]
    public void Pack_FromShipped_Throws()
    {
        var order = Create();
        order.Pack();
        order.Ship("USPS", "tracking-123");
        Assert.Throws<InvalidOperationException>(() => order.Pack());
    }

    [Fact]
    public void Ship_FromPacked_SetsTrackingAndAllLinesShipped()
    {
        var order = Create1042();
        order.Pack();
        order.Ship("USPS", "USPS · 9405 5036 9930 0124 2317");

        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.All(order.Lines, l => Assert.Equal(FulfillmentStatus.Shipped, l.FulfillmentStatus));
    }

    [Fact]
    public void Deliver_FromShipped_Transitions()
    {
        var order = Create();
        order.Pack();
        order.Ship("USPS", null);
        order.Deliver();
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Cancel_FromNewOrPacked_Transitions()
    {
        var fromNew = Create();
        fromNew.Cancel();
        Assert.Equal(OrderStatus.Cancelled, fromNew.Status);

        var fromPacked = Create();
        fromPacked.Pack();
        fromPacked.Cancel();
        Assert.Equal(OrderStatus.Cancelled, fromPacked.Status);
    }

    [Fact]
    public void Cancel_FromDelivered_Throws()
    {
        var order = Create();
        order.Pack();
        order.Ship("USPS", null);
        order.Deliver();
        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void RequestRefund_FromDelivered_AttachesRefundDraft()
    {
        var order = Create();
        order.Pack();
        order.Ship("USPS", null);
        order.Deliver();

        var draft = new RefundDraft(
            "Item arrived chipped",
            "Damage",
            30.00m,
            new List<RefundDraftItem>
            {
                new("MC-VS-001", 1, 86.00m, false, null),
                new("MC-AB-002", 1, 66.00m, true, 30.00m),
            });

        order.RequestRefund(draft);

        Assert.Equal(OrderStatus.RefundRequested, order.Status);
        Assert.NotNull(order.Refund);
        Assert.Equal("Damage", order.Refund!.RestockChoice);
        Assert.Equal(30.00m, order.Refund.Total);
        Assert.Equal(2, order.Refund.Items.Count);
    }

    [Fact]
    public void SubtotalFeeNet_ComputedCorrectly()
    {
        var order = Create1042();

        Assert.Equal(152.00m, order.Subtotal);
        Assert.Equal(152.00m, order.Paid);
        Assert.Equal(6.08m, order.Fee);
        Assert.Equal(136.08m, order.Net);
    }

    [Fact]
    public void UpdateInternalNote_SetsNote()
    {
        var order = Create();
        order.UpdateInternalNote("Ship together if faster.");
        Assert.Equal("Ship together if faster.", order.InternalNote);
    }

    [Fact]
    public void ToggleStar_Flips()
    {
        var order = Create();
        Assert.False(order.Starred);
        order.ToggleStar();
        Assert.True(order.Starred);
        order.ToggleStar();
        Assert.False(order.Starred);
    }
}
