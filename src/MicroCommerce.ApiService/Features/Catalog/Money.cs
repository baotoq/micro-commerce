using Vogen;

namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>
/// An amount in the Marketplace currency, whole Vietnamese đồng (VND has no minor unit in use), never negative.
/// Requests carry it as a number checked by FluentValidation; <see cref="Validate"/> is the backstop. Reading it
/// back from the database or JSON re-runs the rule.
/// </summary>
[ValueObject<long>]
public readonly partial struct Money
{
    private static Validation Validate(long input) =>
        input < 0 ? Validation.Invalid("An amount of money cannot be negative.") : Validation.Ok;
}
