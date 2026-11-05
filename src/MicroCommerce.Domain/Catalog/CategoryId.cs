using Vogen;

namespace MicroCommerce.Domain.Catalog;

/// <summary>Identifies a node in the Marketplace-wide Category tree.</summary>
[ValueObject<Guid>]
public readonly partial struct CategoryId
{
    public static CategoryId New() => From(Guid.CreateVersion7());
}
