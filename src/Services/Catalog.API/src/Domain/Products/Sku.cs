using Vogen;

namespace MicroCommerce.Catalog.Domain.Products;

[ValueObject<string>]
public partial struct Sku
{
    private static Validation Validate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("SKU cannot be empty.")
            : Validation.Ok;

    private static string NormalizeInput(string value) => value.Trim().ToUpperInvariant();
}
