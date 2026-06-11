using Vogen;

namespace MicroCommerce.Catalog.Domain.Customers;

[ValueObject<Guid>]
public partial struct CustomerId
{
    public static CustomerId New() => From(Guid.NewGuid());
}
