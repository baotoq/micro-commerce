using MicroCommerce.Domain.SharedKernel;
using Vogen;

namespace MicroCommerce.Domain.Accounts;

/// <summary>
/// A Merchant's public name: trimmed, 3 to 50 characters (FR-003). Unique across the Marketplace,
/// ignoring letter case; <see cref="Normalized"/> is the form that uniqueness is checked on.
/// </summary>
[ValueObject<string>(throws: typeof(DomainRuleViolation))]
public readonly partial struct ShopName
{
    public const int MinLength = 3;
    public const int MaxLength = 50;

    public string Normalized => Value.ToUpperInvariant();

    public static string Normalize(string name) => name.Trim().ToUpperInvariant();

    private static string NormalizeInput(string input) => input?.Trim() ?? string.Empty;

    private static Validation Validate(string input) =>
        input.Length is >= MinLength and <= MaxLength
            ? Validation.Ok
            : Validation.Invalid($"A Shop name must be {MinLength} to {MaxLength} characters");
}
