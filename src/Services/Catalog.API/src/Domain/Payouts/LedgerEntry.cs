namespace MicroCommerce.Catalog.Domain.Payouts;

public class LedgerEntry
{
    public LedgerEntryId Id { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public decimal Amount { get; private set; }
    public LedgerEntryKind Kind { get; private set; }

    // Display hint referencing an order; not a real foreign key.
    public int? OrderNumber { get; private set; }

    private LedgerEntry() { }

    public LedgerEntry(
        DateTimeOffset occurredAt,
        string label,
        string? subject,
        decimal amount,
        LedgerEntryKind kind,
        int? orderNumber)
    {
        ValidateInvariants(label);

        Id = LedgerEntryId.New();
        OccurredAt = occurredAt;
        Label = label;
        Subject = subject;
        Amount = amount;
        Kind = kind;
        OrderNumber = orderNumber;
    }

    private static void ValidateInvariants(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Label required.", nameof(label));
    }
}
