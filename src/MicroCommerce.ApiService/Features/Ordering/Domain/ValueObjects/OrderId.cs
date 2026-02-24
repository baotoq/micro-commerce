using Vogen;

namespace MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct OrderId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("OrderId cannot be empty.");

    public static OrderId New() => From(Guid.CreateVersion7());
}
