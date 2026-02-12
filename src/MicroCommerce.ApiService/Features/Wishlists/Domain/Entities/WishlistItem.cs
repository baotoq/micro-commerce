using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Features.Wishlists.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Wishlists.Domain.Entities;

/// <summary>
/// WishlistItem entity representing a saved product in a user's wishlist.
/// This is a simple entity (not an aggregate root) with no domain events.
/// Uses optimistic concurrency via PostgreSQL xmin column.
/// </summary>
public sealed class WishlistItem
{
    /// <summary>
    /// Unique identifier for the wishlist item.
    /// </summary>
    public WishlistItemId Id { get; private set; } = null!;

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

    /// <summary>
    /// Concurrency token mapped to PostgreSQL xmin system column.
    /// </summary>
    [Timestamp]
    public uint Version { get; private set; }

    // EF Core constructor
    private WishlistItem()
    {
    }

    /// <summary>
    /// Factory method for creating a new wishlist item.
    /// </summary>
    public static WishlistItem Create(Guid userId, Guid productId)
    {
        return new WishlistItem
        {
            Id = WishlistItemId.New(),
            UserId = userId,
            ProductId = productId,
            AddedAt = DateTimeOffset.UtcNow
        };
    }
}
