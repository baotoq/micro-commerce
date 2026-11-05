using MicroCommerce.Domain.SharedKernel;
using Vogen;

namespace MicroCommerce.Domain.Catalog;

/// <summary>
/// A price in whole units of the single Marketplace currency, which has no fractional units. Above
/// zero and at most JavaScript's <c>Number.MAX_SAFE_INTEGER</c>, so the web app reads it exactly (FR-012).
/// </summary>
[ValueObject<long>(throws: typeof(DomainRuleViolation))]
public readonly partial struct Money
{
    public const long MaxValue = 9_007_199_254_740_991L;

    private static Validation Validate(long input) =>
        input is > 0 and <= MaxValue
            ? Validation.Ok
            : Validation.Invalid($"A price must be above 0 and at most {MaxValue}");
}
