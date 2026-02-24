using Vogen;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct OrderItemId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("OrderItemId cannot be empty.");

    public static OrderItemId New() => From(Guid.CreateVersion7());
}
