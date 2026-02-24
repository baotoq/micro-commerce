using Vogen;

namespace MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct UserProfileId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("UserProfileId cannot be empty.");

    public static UserProfileId New() => From(Guid.CreateVersion7());
}
