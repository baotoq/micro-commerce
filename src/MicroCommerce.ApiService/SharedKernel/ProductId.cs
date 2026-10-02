using Vogen;

namespace MicroCommerce.ApiService.SharedKernel;

/// <summary>Identifies a Product across modules (ADR-0005).</summary>
[ValueObject<Guid>]
public readonly partial struct ProductId
{
    public static ProductId New() => From(Guid.CreateVersion7());
}
