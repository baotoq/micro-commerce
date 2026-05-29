using Vogen;

namespace MicroCommerce.Catalog.Domain.Promotions;

[ValueObject<string>]
public partial struct PromotionCode
{
    private static Validation Validate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("Promotion code cannot be empty.")
            : Validation.Ok;

    private static string NormalizeInput(string value) => value.Trim().ToUpperInvariant();
}
