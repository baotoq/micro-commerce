using Vogen;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct CategoryId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("CategoryId cannot be empty.");

    public static CategoryId New() => From(Guid.CreateVersion7());
}
