using Vogen;

namespace MicroCommerce.Catalog.Domain.Customers;

[ValueObject<string>]
public partial struct Email
{
    private static Validation Validate(string value) =>
        string.IsNullOrWhiteSpace(value) || !value.Contains('@')
            ? Validation.Invalid("Email must be a non-empty address containing '@'.")
            : Validation.Ok;

    private static string NormalizeInput(string value) => value.Trim().ToLowerInvariant();
}
