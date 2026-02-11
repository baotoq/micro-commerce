using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Features.Reviews.Domain.Events;
using MicroCommerce.ApiService.Features.Reviews.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Reviews.Domain.Entities;

/// <summary>
/// Review aggregate root for the reviews domain.
/// Manages product reviews with rating and text.
/// Uses optimistic concurrency via PostgreSQL xmin column.
/// </summary>
public sealed class Review : BaseAggregateRoot<ReviewId>
{
    /// <summary>
    /// Product being reviewed.
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// User who created the review (Keycloak user ID from 'sub' claim).
    /// </summary>
    public Guid UserId { get; private set; }

    public Rating Rating { get; private set; } = null!;

    public ReviewText Text { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Concurrency token mapped to PostgreSQL xmin system column.
    /// </summary>
    [Timestamp]
    public uint Version { get; private set; }

    // EF Core constructor
    private Review(ReviewId id) : base(id)
    {
    }

    /// <summary>
    /// Factory method for creating a new review.
    /// </summary>
    public static Review Create(Guid productId, Guid userId, int rating, string text)
    {
        var now = DateTimeOffset.UtcNow;
        var review = new Review(ReviewId.New())
        {
            ProductId = productId,
            UserId = userId,
            Rating = Rating.Create(rating),
            Text = ReviewText.Create(text),
            CreatedAt = now,
            UpdatedAt = now
        };

        review.AddDomainEvent(new ReviewCreatedDomainEvent(review.Id.Value, productId));

        return review;
    }

    /// <summary>
    /// Updates the review rating and text.
    /// </summary>
    public void Update(int rating, string text)
    {
        Rating = Rating.Create(rating);
        Text = ReviewText.Create(text);
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new ReviewUpdatedDomainEvent(Id.Value, ProductId));
    }

    /// <summary>
    /// Marks the review for deletion by raising the deleted event.
    /// </summary>
    public void MarkDeleted()
    {
        AddDomainEvent(new ReviewDeletedDomainEvent(Id.Value, ProductId));
    }
}
