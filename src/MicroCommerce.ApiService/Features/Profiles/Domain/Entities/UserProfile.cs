using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Features.Profiles.Domain.Events;
using MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Profiles.Domain.Entities;

/// <summary>
/// UserProfile aggregate root for the profiles domain.
/// Manages display name, avatar, and address collection with default address invariant.
/// Uses optimistic concurrency via PostgreSQL xmin column.
/// </summary>
public sealed class UserProfile : BaseAggregateRoot<UserProfileId>
{
    private readonly List<Address> _addresses = [];

    /// <summary>
    /// Keycloak user ID from 'sub' claim - has unique index to prevent duplicate profiles.
    /// </summary>
    public Guid UserId { get; private set; }

    public DisplayName DisplayName { get; private set; }

    public string? AvatarUrl { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Concurrency token mapped to PostgreSQL xmin system column.
    /// </summary>
    [Timestamp]
    public uint Version { get; private set; }

    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

    // EF Core constructor
    private UserProfile(UserProfileId id) : base(id)
    {
        DisplayName = default;
    }

    /// <summary>
    /// Factory method for creating a new user profile.
    /// </summary>
    public static UserProfile Create(Guid userId, string displayName)
    {
        var now = DateTimeOffset.UtcNow;
        var profile = new UserProfile(UserProfileId.New())
        {
            UserId = userId,
            DisplayName = DisplayName.Create(displayName),
            CreatedAt = now,
            UpdatedAt = now
        };

        profile.AddDomainEvent(new ProfileCreatedDomainEvent(profile.Id, userId));

        return profile;
    }

    /// <summary>
    /// Updates the display name.
    /// </summary>
    public void UpdateDisplayName(string displayName)
    {
        DisplayName = DisplayName.Create(displayName);
        Touch();
    }

    /// <summary>
    /// Sets the avatar URL.
    /// </summary>
    public void SetAvatar(string avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(avatarUrl))
            throw new ArgumentException("Avatar URL cannot be null or empty.", nameof(avatarUrl));

        AvatarUrl = avatarUrl.Trim();
        Touch();
    }

    /// <summary>
    /// Removes the avatar.
    /// </summary>
    public void RemoveAvatar()
    {
        AvatarUrl = null;
        Touch();
    }

    /// <summary>
    /// Adds a new address to the collection. If setAsDefault is true or this is the first address, it becomes the default.
    /// </summary>
    public AddressId AddAddress(string name, string street, string city, string state, string zipCode, string country, bool setAsDefault = false)
    {
        var address = Address.Create(name, street, city, state, zipCode, country);

        if (setAsDefault || _addresses.Count == 0)
        {
            // Clear existing defaults
            foreach (var addr in _addresses)
            {
                addr.ClearDefault();
            }
            address.SetAsDefault();
        }

        _addresses.Add(address);
        Touch();

        return address.Id;
    }

    /// <summary>
    /// Updates an existing address.
    /// </summary>
    public void UpdateAddress(AddressId addressId, string name, string street, string city, string state, string zipCode, string country)
    {
        var existingAddress = FindAddress(addressId);
        var wasDefault = existingAddress.IsDefault;

        // Remove old and add updated address
        _addresses.Remove(existingAddress);
        var updatedAddress = Address.Create(name, street, city, state, zipCode, country);

        if (wasDefault)
        {
            updatedAddress.SetAsDefault();
        }

        _addresses.Add(updatedAddress);
        Touch();
    }

    /// <summary>
    /// Deletes an address. If it was the default, sets the first remaining address as default.
    /// </summary>
    public void DeleteAddress(AddressId addressId)
    {
        var address = FindAddress(addressId);
        var wasDefault = address.IsDefault;

        _addresses.Remove(address);

        if (wasDefault && _addresses.Count > 0)
        {
            _addresses[0].SetAsDefault();
        }

        Touch();
    }

    /// <summary>
    /// Sets the specified address as the default, clearing all others.
    /// </summary>
    public void SetDefaultAddress(AddressId addressId)
    {
        var targetAddress = FindAddress(addressId);

        foreach (var addr in _addresses)
        {
            addr.ClearDefault();
        }

        targetAddress.SetAsDefault();
        Touch();
    }

    /// <summary>
    /// Updates modification timestamp and raises domain event.
    /// </summary>
    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new ProfileUpdatedDomainEvent(Id, UserId));
    }

    /// <summary>
    /// Finds an address by ID or throws.
    /// </summary>
    private Address FindAddress(AddressId addressId)
    {
        return _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new InvalidOperationException($"Address '{addressId}' not found.");
    }
}
