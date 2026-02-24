using Vogen;

namespace MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct CartId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("CartId cannot be empty.");

    public static CartId New() => From(Guid.CreateVersion7());
}
