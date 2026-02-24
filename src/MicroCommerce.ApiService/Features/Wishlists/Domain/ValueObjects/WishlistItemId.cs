using Vogen;

namespace MicroCommerce.ApiService.Features.Wishlists.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct WishlistItemId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("WishlistItemId cannot be empty.");

    public static WishlistItemId New() => From(Guid.CreateVersion7());
}
