using Vogen;

namespace MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct CartItemId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("CartItemId cannot be empty.");

    public static CartItemId New() => From(Guid.CreateVersion7());
}
