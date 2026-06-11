namespace MicroCommerce.Catalog.Domain.Orders;

public class RefundDraftItem
{
    public string Sku { get; private set; } = string.Empty;
    public int Qty { get; private set; }
    public decimal Amount { get; private set; }
    public bool Selected { get; private set; }
    public decimal? PartialAmount { get; private set; }

    private RefundDraftItem() { }

    public RefundDraftItem(
        string sku,
        int qty,
        decimal amount,
        bool selected,
        decimal? partialAmount)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU required.", nameof(sku));
        if (qty < 1)
            throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be at least 1.");
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");
        if (partialAmount is < 0)
            throw new ArgumentOutOfRangeException(nameof(partialAmount), "Partial amount cannot be negative.");

        Sku = sku;
        Qty = qty;
        Amount = amount;
        Selected = selected;
        PartialAmount = partialAmount;
    }
}
