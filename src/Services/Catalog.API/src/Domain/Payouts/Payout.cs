namespace MicroCommerce.Catalog.Domain.Payouts;

public class Payout
{
    public PayoutId Id { get; private set; }
    public DateTimeOffset SentAt { get; private set; }
    public decimal Amount { get; private set; }
    public string Period { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public bool IsPending { get; private set; }

    private Payout() { }

    public Payout(
        DateTimeOffset sentAt,
        decimal amount,
        string period,
        string destination,
        bool isPending)
    {
        ValidateInvariants(amount, period, destination);

        Id = PayoutId.New();
        SentAt = sentAt;
        Amount = amount;
        Period = period;
        Destination = destination;
        IsPending = isPending;
    }

    public void MarkSent()
    {
        IsPending = false;
    }

    private static void ValidateInvariants(decimal amount, string period, string destination)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
        if (string.IsNullOrWhiteSpace(period))
            throw new ArgumentException("Period required.", nameof(period));
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("Destination required.", nameof(destination));
    }
}
