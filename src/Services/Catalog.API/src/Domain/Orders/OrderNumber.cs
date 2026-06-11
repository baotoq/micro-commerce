using Vogen;

namespace MicroCommerce.Catalog.Domain.Orders;

[ValueObject<int>]
public partial struct OrderNumber
{
    private static Validation Validate(int value) =>
        value > 0
            ? Validation.Ok
            : Validation.Invalid("Order number must be positive.");
}
