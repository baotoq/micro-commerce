using Vogen;

namespace MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct AddressId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("AddressId cannot be empty.");

    public static AddressId New() => From(Guid.CreateVersion7());
}
