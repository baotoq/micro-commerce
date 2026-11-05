using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Accounts;

/// <summary>
/// A selling business on the Marketplace, owned by exactly one Account (FR-001, FR-002). It can sell
/// as soon as it is opened. One Merchant per Account and unique Shop names are enforced by the
/// database's unique indexes, which the handlers map to conflicts (research R3).
/// </summary>
public sealed class Merchant : AggregateRoot<MerchantId>
{
    public const int MaxDescriptionLength = 2000;

    private Merchant()
    {
    }

    public AccountId AccountId { get; private init; }

    public ShopName ShopName { get; private set; }

    /// <summary><see cref="ShopName"/> in upper case: the form the unique index is on.</summary>
    public string NormalizedShopName { get; private set; } = null!;

    public string? Description { get; private set; }

    public PickupAddress PickupAddress { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private init; }

    /// <summary>Bumped by every change; the optimistic concurrency token for Shop edits.</summary>
    public int Version { get; private set; }

    public static Merchant Open(
        MerchantId id,
        AccountId accountId,
        ShopName shopName,
        string? description,
        PickupAddress pickupAddress,
        DateTimeOffset now)
    {
        var merchant = new Merchant { Id = id, AccountId = accountId, CreatedAt = now, Version = 1 };
        merchant.SetProfile(shopName, description, pickupAddress);
        return merchant;
    }

    /// <summary>Changes the Shop profile. Changing only the letter case of its own name is allowed.</summary>
    public void ChangeProfile(ShopName shopName, string? description, PickupAddress pickupAddress)
    {
        SetProfile(shopName, description, pickupAddress);
        Version++;
    }

    private void SetProfile(ShopName shopName, string? description, PickupAddress pickupAddress)
    {
        var trimmed = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (trimmed?.Length > MaxDescriptionLength)
            throw new DomainRuleViolation($"A Shop description must be at most {MaxDescriptionLength} characters");

        ShopName = shopName;
        NormalizedShopName = shopName.Normalized;
        Description = trimmed;
        PickupAddress = pickupAddress;
    }
}
