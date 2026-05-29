using Vogen;

namespace MicroCommerce.Catalog.Domain.Promotions;

[ValueObject<Guid>]
public partial struct PromotionId
{
    public static PromotionId New() => From(Guid.NewGuid());
}
