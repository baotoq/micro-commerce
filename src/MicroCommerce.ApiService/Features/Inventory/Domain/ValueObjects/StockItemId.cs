using Vogen;

namespace MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct StockItemId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("StockItemId cannot be empty.");

    public static StockItemId New() => From(Guid.CreateVersion7());
}
