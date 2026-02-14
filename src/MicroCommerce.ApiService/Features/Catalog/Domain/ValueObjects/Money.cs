using Ardalis.GuardClauses;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency.
/// </summary>
public readonly record struct Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        Guard.Against.Negative(amount, nameof(amount));
        Guard.Against.NullOrWhiteSpace(currency, nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public override string ToString() => $"{Currency} {Amount:F2}";

    public string Format() => Amount.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
}
