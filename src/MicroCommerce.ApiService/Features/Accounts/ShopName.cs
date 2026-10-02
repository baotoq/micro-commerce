using Vogen;

namespace MicroCommerce.ApiService.Features.Accounts;

/// <summary>
/// A Shop's name: trimmed, non-empty, at most <see cref="MaxLength"/> characters, and unique ignoring case.
/// Requests carry it as a string checked by FluentValidation; <see cref="Validate"/> is the backstop. Public
/// only because Accounts' responses carry it; other modules don't use it (ADR-0005). Reading it back from the
/// database or JSON re-runs the rules, so a stored name that breaks them fails to load.
/// </summary>
[ValueObject<string>]
public readonly partial struct ShopName
{
    public const int MaxLength = 100;

    /// <summary>The name with case folded away; two Shop names collide when these are equal.</summary>
    public string Normalized => Value.ToUpperInvariant();

    private static string NormalizeInput(string input) => input.Trim();

    private static Validation Validate(string input) =>
        input.Length == 0 ? Validation.Invalid("A Shop name cannot be empty.")
        : input.Length > MaxLength ? Validation.Invalid($"A Shop name cannot be longer than {MaxLength} characters.")
        : Validation.Ok;
}
