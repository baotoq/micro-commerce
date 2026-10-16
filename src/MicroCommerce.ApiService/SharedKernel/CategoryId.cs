using Vogen;

namespace MicroCommerce.ApiService.SharedKernel;

/// <summary>Identifies a Category of the Marketplace-wide Category tree (ADR-0005).</summary>
[ValueObject<Guid>]
public readonly partial struct CategoryId
{
    public static CategoryId New() => From(Guid.CreateVersion7());
}
