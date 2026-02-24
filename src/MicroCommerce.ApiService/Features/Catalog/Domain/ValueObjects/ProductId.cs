using Vogen;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct ProductId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("ProductId cannot be empty.");

    public static ProductId New() => From(Guid.CreateVersion7());
}
