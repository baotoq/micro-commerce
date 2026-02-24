using Vogen;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct ReservationId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("ReservationId cannot be empty.");

    public static ReservationId New() => From(Guid.CreateVersion7());
}
