namespace MicroCommerce.Catalog.Domain.Promotions;

public class Promotion
{
    public PromotionId Id { get; private set; }
    public PromotionCode Code { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DiscountKind Kind { get; private set; }
    public int? PercentValue { get; private set; }
    public decimal? FixedAmount { get; private set; }
    public decimal? MinOrderAmount { get; private set; }
    public DateTimeOffset? StartsAt { get; private set; }
    public DateTimeOffset? EndsAt { get; private set; }
    public PromotionStatus Status { get; private set; }

    private Promotion() { }

    public Promotion(
        PromotionCode code,
        string description,
        DiscountKind kind,
        int? percentValue,
        decimal? fixedAmount,
        decimal? minOrderAmount,
        DateTimeOffset? startsAt,
        DateTimeOffset? endsAt)
    {
        ValidateInvariants(description, kind, percentValue, fixedAmount, minOrderAmount, startsAt, endsAt);

        Id = PromotionId.New();
        Code = code;
        Description = description;
        Kind = kind;
        PercentValue = percentValue;
        FixedAmount = fixedAmount;
        MinOrderAmount = minOrderAmount;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Status = PromotionStatus.Draft;
    }

    public void Activate()
    {
        if (Status != PromotionStatus.Draft)
            throw new InvalidOperationException($"Cannot activate promotion in status '{Status}'.");
        Status = PromotionStatus.Active;
    }

    public void End()
    {
        if (Status != PromotionStatus.Active)
            throw new InvalidOperationException($"Cannot end promotion in status '{Status}'.");
        Status = PromotionStatus.Ended;
    }

    public void UpdateDetails(
        string description,
        DiscountKind kind,
        int? percentValue,
        decimal? fixedAmount,
        decimal? minOrderAmount,
        DateTimeOffset? startsAt,
        DateTimeOffset? endsAt)
    {
        if (Status == PromotionStatus.Ended)
            throw new InvalidOperationException("Cannot update an ended promotion.");

        ValidateInvariants(description, kind, percentValue, fixedAmount, minOrderAmount, startsAt, endsAt);

        Description = description;
        Kind = kind;
        PercentValue = percentValue;
        FixedAmount = fixedAmount;
        MinOrderAmount = minOrderAmount;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    private static void ValidateInvariants(
        string description,
        DiscountKind kind,
        int? percentValue,
        decimal? fixedAmount,
        decimal? minOrderAmount,
        DateTimeOffset? startsAt,
        DateTimeOffset? endsAt)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description required.", nameof(description));
        if (description.Length > 200)
            throw new ArgumentException("Description must be 200 characters or fewer.", nameof(description));

        switch (kind)
        {
            case DiscountKind.Percentage:
                if (fixedAmount is not null)
                    throw new ArgumentException("Percentage promotions must not set FixedAmount.", nameof(fixedAmount));
                if (percentValue is null)
                    throw new ArgumentException("Percentage promotions require PercentValue.", nameof(percentValue));
                if (percentValue is < 1 or > 100)
                    throw new ArgumentOutOfRangeException(nameof(percentValue), "PercentValue must be between 1 and 100.");
                break;
            case DiscountKind.FixedAmount:
                if (percentValue is not null)
                    throw new ArgumentException("Fixed promotions must not set PercentValue.", nameof(percentValue));
                if (fixedAmount is null)
                    throw new ArgumentException("Fixed promotions require FixedAmount.", nameof(fixedAmount));
                if (fixedAmount <= 0)
                    throw new ArgumentOutOfRangeException(nameof(fixedAmount), "FixedAmount must be positive.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown discount kind.");
        }

        if (minOrderAmount is < 0)
            throw new ArgumentOutOfRangeException(nameof(minOrderAmount), "MinOrderAmount cannot be negative.");

        if (startsAt is not null && endsAt is not null && startsAt >= endsAt)
            throw new ArgumentException("StartsAt must be before EndsAt.", nameof(startsAt));
    }
}
