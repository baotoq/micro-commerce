using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Accounts;

/// <summary>
/// Where a Merchant's parcels ship from (FR-004). The province is what a Carrier will quote Shipping
/// Fees from. Never shown to Buyers (FR-031).
/// </summary>
public sealed record PickupAddress
{
    public const int MaxStreetLength = 200;
    public const int MaxDistrictLength = 100;
    public const int MaxProvinceLength = 100;

    public PickupAddress(string street, string district, string province)
    {
        Street = Required(street, MaxStreetLength, "street");
        District = Required(district, MaxDistrictLength, "district");
        Province = Required(province, MaxProvinceLength, "province");
    }

    public string Street { get; }

    public string District { get; }

    public string Province { get; }

    private static string Required(string value, int maxLength, string part)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || trimmed.Length > maxLength)
            throw new DomainRuleViolation($"A Pickup Address {part} must be 1 to {maxLength} characters");
        return trimmed;
    }
}
