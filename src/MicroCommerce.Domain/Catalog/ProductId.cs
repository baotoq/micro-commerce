using Vogen;

namespace MicroCommerce.Domain.Catalog;

/// <summary>Identifies a Product.</summary>
[ValueObject<Guid>]
public readonly partial struct ProductId
{
    public static ProductId New() => From(Guid.CreateVersion7());
}
