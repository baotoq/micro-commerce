using Vogen;

namespace MicroCommerce.ApiService.SharedKernel;

/// <summary>Identifies one purchasable Variant of a Product across modules (ADR-0005).</summary>
[ValueObject<Guid>]
public readonly partial struct VariantId
{
    public static VariantId New() => From(Guid.CreateVersion7());
}
