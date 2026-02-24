using Vogen;

namespace MicroCommerce.ApiService.Features.Reviews.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct ReviewId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("ReviewId cannot be empty.");

    public static ReviewId New() => From(Guid.CreateVersion7());
}
