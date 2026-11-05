using Vogen;

namespace MicroCommerce.Domain.Catalog;

/// <summary>Identifies a Variant. It stays the same while its combination of Option values survives an edit.</summary>
[ValueObject<Guid>]
public readonly partial struct VariantId
{
    public static VariantId New() => From(Guid.CreateVersion7());
}
