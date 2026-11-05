using Vogen;

namespace MicroCommerce.Domain.SharedKernel;

/// <summary>
/// Identifies a Merchant. Shared by Accounts, which owns the Merchant, and Catalog, whose Products
/// belong to one (research R1).
/// </summary>
[ValueObject<Guid>]
public readonly partial struct MerchantId
{
    public static MerchantId New() => From(Guid.CreateVersion7());
}
