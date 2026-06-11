namespace MicroCommerce.Catalog.Domain.Orders;

public class RefundDraft
{
    private readonly List<RefundDraftItem> _items = new();

    public string Reason { get; private set; } = string.Empty;
    public string RestockChoice { get; private set; } = string.Empty;
    public decimal Total { get; private set; }
    public IReadOnlyList<RefundDraftItem> Items => _items;

    private RefundDraft() { }

    public RefundDraft(
        string reason,
        string restockChoice,
        decimal total,
        IReadOnlyList<RefundDraftItem> items)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason required.", nameof(reason));
        if (string.IsNullOrWhiteSpace(restockChoice))
            throw new ArgumentException("Restock choice required.", nameof(restockChoice));
        if (total < 0)
            throw new ArgumentOutOfRangeException(nameof(total), "Total cannot be negative.");
        ArgumentNullException.ThrowIfNull(items);

        Reason = reason;
        RestockChoice = restockChoice;
        Total = total;
        _items.AddRange(items);
    }
}
