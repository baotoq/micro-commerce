namespace MicroCommerce.Catalog.Domain.Orders;

public class OrderLine
{
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string Tone { get; private set; } = string.Empty;
    public int Qty { get; private set; }
    public decimal UnitPrice { get; private set; }
    public FulfillmentStatus FulfillmentStatus { get; private set; }
    public string? Tracking { get; private set; }
    public string? RestockNote { get; private set; }

    private OrderLine() { }

    public OrderLine(
        string sku,
        string productName,
        string tone,
        int qty,
        decimal unitPrice,
        FulfillmentStatus fulfillmentStatus,
        string? tracking,
        string? restockNote)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU required.", nameof(sku));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name required.", nameof(productName));
        if (qty < 1)
            throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be at least 1.");
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

        Sku = sku;
        ProductName = productName;
        Tone = tone;
        Qty = qty;
        UnitPrice = unitPrice;
        FulfillmentStatus = fulfillmentStatus;
        Tracking = tracking;
        RestockNote = restockNote;
    }

    public void MarkShipped(string? tracking)
    {
        FulfillmentStatus = FulfillmentStatus.Shipped;
        if (tracking is not null)
            Tracking = tracking;
    }
}
