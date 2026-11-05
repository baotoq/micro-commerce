using Vogen;

namespace MicroCommerce.Domain.Catalog;

/// <summary>Identifies a Product image. It is also part of the image's blob name.</summary>
[ValueObject<Guid>]
public readonly partial struct ProductImageId
{
    public static ProductImageId New() => From(Guid.CreateVersion7());
}
