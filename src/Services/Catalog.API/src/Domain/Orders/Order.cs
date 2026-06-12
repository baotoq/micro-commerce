namespace MicroCommerce.Catalog.Domain.Orders;

public class Order
{
    private readonly List<OrderLine> _lines = new();
    private readonly List<OrderTimelineEntry> _timeline = new();

    // Identity / status
    public OrderId Id { get; private set; }
    public int Number { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public bool Starred { get; private set; }

    // Customer snapshot
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string CityState { get; private set; } = string.Empty;
    public string ShipLine1 { get; private set; } = string.Empty;
    public string ShipLine2 { get; private set; } = string.Empty;
    public bool BillSameAsShip { get; private set; }

    // Money / meta
    // Seeded verbatim from mock; not derivable from Lines — mock uses summary shorthand notation
    public string ItemsSummary { get; private set; } = string.Empty;
    public string ShippingMethod { get; private set; } = string.Empty;
    public decimal ShippingPaid { get; private set; }
    public decimal Tax { get; private set; }
    public decimal FeePct { get; private set; } = 4m;
    public string PaymentBrand { get; private set; } = string.Empty;
    public string PaymentLastFour { get; private set; } = string.Empty;
    public string? LabelCarrier { get; private set; }
    public decimal LabelCost { get; private set; }
    public string? LabelWeightLabel { get; private set; }

    public string InternalNote { get; private set; } = string.Empty;

    // Storefront promo discount (applies to item subtotal only; see spec §7)
    public string? DiscountCode { get; private set; }
    public decimal DiscountAmount { get; private set; }

    // Owned collections
    public IReadOnlyList<OrderLine> Lines => _lines;
    public IReadOnlyList<OrderTimelineEntry> Timeline => _timeline;
    public RefundDraft? Refund { get; private set; }

    // Computed (not stored)
    public decimal Subtotal => _lines.Sum(l => l.UnitPrice * l.Qty);
    public decimal Paid => Subtotal - DiscountAmount + ShippingPaid + Tax;
    public decimal Fee => Math.Round(Paid * FeePct / 100m, 2, MidpointRounding.AwayFromZero);
    public decimal Net => Paid - Fee - LabelCost;

    private Order() { }

    public Order(
        int number,
        DateTimeOffset placedAt,
        string customerName,
        string customerEmail,
        string cityState,
        string shipLine1,
        string shipLine2,
        bool billSameAsShip,
        string itemsSummary,
        string shippingMethod,
        decimal shippingPaid,
        decimal tax,
        decimal feePct,
        string paymentBrand,
        string paymentLastFour,
        string? labelCarrier,
        decimal labelCost,
        string? labelWeightLabel,
        string internalNote,
        IReadOnlyList<OrderLine> lines)
    {
        ValidateInvariants(
            customerName, customerEmail, cityState, shippingPaid, tax, labelCost, lines);

        Id = OrderId.New();
        Number = number;
        PlacedAt = placedAt;
        Status = OrderStatus.New;
        Starred = false;

        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CityState = cityState;
        ShipLine1 = shipLine1;
        ShipLine2 = shipLine2;
        BillSameAsShip = billSameAsShip;

        ItemsSummary = itemsSummary;
        ShippingMethod = shippingMethod;
        ShippingPaid = shippingPaid;
        Tax = tax;
        FeePct = feePct;
        PaymentBrand = paymentBrand;
        PaymentLastFour = paymentLastFour;
        LabelCarrier = labelCarrier;
        LabelCost = labelCost;
        LabelWeightLabel = labelWeightLabel;

        InternalNote = internalNote;

        _lines.AddRange(lines);
    }

    public void Pack()
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException($"Cannot pack order in status '{Status}'.");
        Status = OrderStatus.Packed;
    }

    public void Ship(string carrier, string? trackingNote)
    {
        if (Status != OrderStatus.Packed)
            throw new InvalidOperationException($"Cannot ship order in status '{Status}'.");
        if (string.IsNullOrWhiteSpace(carrier))
            throw new ArgumentException("Carrier required.", nameof(carrier));

        Status = OrderStatus.Shipped;
        foreach (var line in _lines)
            line.MarkShipped(trackingNote);
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver order in status '{Status}'.");
        Status = OrderStatus.Delivered;
    }

    public void Cancel()
    {
        if (Status is not (OrderStatus.New or OrderStatus.Packed))
            throw new InvalidOperationException($"Cannot cancel order in status '{Status}'.");
        Status = OrderStatus.Cancelled;
    }

    public void RequestRefund(RefundDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        if (Status is not (OrderStatus.Shipped or OrderStatus.Delivered))
            throw new InvalidOperationException($"Cannot request refund for order in status '{Status}'.");

        Refund = draft;
        Status = OrderStatus.RefundRequested;
    }

    public void UpdateInternalNote(string note)
    {
        InternalNote = note ?? string.Empty;
    }

    public void ToggleStar()
    {
        Starred = !Starred;
    }

    public void ApplyDiscount(string code, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Discount code required.", nameof(code));
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Discount cannot be negative.");
        if (amount > Subtotal)
            throw new ArgumentOutOfRangeException(nameof(amount), "Discount cannot exceed the item subtotal.");

        DiscountCode = code;
        DiscountAmount = amount;
    }

    public void AppendTimeline(OrderTimelineEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _timeline.Add(entry);
    }

    private static void ValidateInvariants(
        string customerName,
        string customerEmail,
        string cityState,
        decimal shippingPaid,
        decimal tax,
        decimal labelCost,
        IReadOnlyList<OrderLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
            throw new ArgumentException("Order requires at least one line.", nameof(lines));

        foreach (var line in lines)
        {
            if (line.Qty < 1)
                throw new ArgumentOutOfRangeException(nameof(lines), "Line quantity must be at least 1.");
            if (line.UnitPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(lines), "Line unit price cannot be negative.");
        }

        if (shippingPaid < 0)
            throw new ArgumentOutOfRangeException(nameof(shippingPaid), "ShippingPaid cannot be negative.");
        if (tax < 0)
            throw new ArgumentOutOfRangeException(nameof(tax), "Tax cannot be negative.");
        if (labelCost < 0)
            throw new ArgumentOutOfRangeException(nameof(labelCost), "LabelCost cannot be negative.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name required.", nameof(customerName));
        if (string.IsNullOrWhiteSpace(customerEmail))
            throw new ArgumentException("Customer email required.", nameof(customerEmail));
        if (string.IsNullOrWhiteSpace(cityState))
            throw new ArgumentException("City/state required.", nameof(cityState));
    }
}
