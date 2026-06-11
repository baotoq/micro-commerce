using Vogen;

namespace MicroCommerce.Catalog.Domain.Orders;

[ValueObject<Guid>]
public partial struct OrderId
{
    public static OrderId New() => From(Guid.NewGuid());
}
