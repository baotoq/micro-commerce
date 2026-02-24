using Vogen;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct AdjustmentId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("AdjustmentId cannot be empty.");

    public static AdjustmentId New() => From(Guid.CreateVersion7());
}
