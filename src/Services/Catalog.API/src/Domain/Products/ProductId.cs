using Vogen;

namespace MicroCommerce.Catalog.Domain.Products;

[ValueObject<Guid>]
public partial struct ProductId
{
    public static ProductId New() => From(Guid.NewGuid());
}
