using Vogen;

namespace MicroCommerce.Catalog.Domain.Payouts;

[ValueObject<Guid>]
public partial struct LedgerEntryId
{
    public static LedgerEntryId New() => From(Guid.NewGuid());
}
