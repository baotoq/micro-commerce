using Vogen;

namespace MicroCommerce.ApiService.SharedKernel;

/// <summary>Identifies a Merchant across modules (ADR-0005).</summary>
[ValueObject<Guid>]
public readonly partial struct MerchantId
{
    public static MerchantId New() => From(Guid.CreateVersion7());
}
