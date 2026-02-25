using MicroCommerce.ApiService.Features.Wishlists.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Wishlists.Domain.Entities;

/// <summary>
/// WishlistItem entity representing a saved product in a user's wishlist.
/// This is a simple entity (not an aggregate root) with no domain events.
/// </summary>
public sealed class WishlistItem : Entity<WishlistItemId>, IConcurrencyToken
{
    /// <summary>
    /// User who saved the item (Keycloak user ID from 'sub' claim).
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Product that was saved to the wishlist.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// When the item was added to the wishlist.
    /// </summary>
    public DateTimeOffset AddedAt { get; private set; }

    public int Version { get; set; }

    // EF Core constructor
    private WishlistItem()
    {
    }

    private WishlistItem(WishlistItemId id) : base(id)
    {
    }

    /// <summary>
    /// Factory method for creating a new wishlist item.
    /// </summary>
    public static WishlistItem Create(Guid userId, Guid productId)
    {
        return new WishlistItem(WishlistItemId.New())
        {
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTimeOffset.UtcNow
        };
    }
}
