using Vogen;

namespace MicroCommerce.Catalog.Domain.Payouts;

[ValueObject<Guid>]
public partial struct PayoutId
{
    public static PayoutId New() => From(Guid.NewGuid());
}
