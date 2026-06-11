using Vogen;

namespace MicroCommerce.Catalog.Domain.Marketing;

[ValueObject<Guid>]
public partial struct CampaignId
{
    public static CampaignId New() => From(Guid.NewGuid());
}
