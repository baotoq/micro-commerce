using Ardalis.GuardClauses;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

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

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:F2}";

    public string Format() => Amount.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
}

