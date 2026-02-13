using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Reviews.Application.Commands.DeleteReview;

public sealed record DeleteReviewCommand(
    Guid UserId,
    Guid ReviewId) : IRequest;

public sealed class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand>
{
    private readonly ReviewsDbContext _reviewsContext;
    private readonly CatalogDbContext _catalogContext;

    public DeleteReviewCommandHandler(
        ReviewsDbContext reviewsContext,
        CatalogDbContext catalogContext)
    {
        _reviewsContext = reviewsContext;
        _catalogContext = catalogContext;
    }

    public async Task Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        // Load review by ReviewId
        var review = await _reviewsContext.Reviews
            .FirstOrDefaultAsync(r => r.Id.Value == request.ReviewId, cancellationToken);

        if (review is null)
        {
            throw new NotFoundException($"Review '{request.ReviewId}' not found.");
        }

        // Verify ownership
        if (review.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You can only delete your own reviews.");
        }

        // Store ProductId before deleting
        var productId = review.ProductId;

        // Mark deleted (raises domain event) and remove
        review.MarkDeleted();
        _reviewsContext.Reviews.Remove(review);
        await _reviewsContext.SaveChangesAsync(cancellationToken);

        // Recalculate aggregate ratings (handles zero reviews case)
        await RecalculateProductRatings(productId, cancellationToken);
    }

    private async Task RecalculateProductRatings(Guid productId, CancellationToken cancellationToken)
    {
        // Calculate average rating and count from all reviews for this product
        var reviews = await _reviewsContext.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync(cancellationToken);

        var reviewCount = reviews.Count;
        var averageRating = reviewCount > 0
            ? (decimal?)reviews.Average(r => r.Rating.Value)
            : null;

        // Load product and update stats (if no reviews, sets to null/0)
        var product = await _catalogContext.Products
            .FirstOrDefaultAsync(p => p.Id == new ProductId(productId), cancellationToken);

        if (product is not null)
        {
            product.UpdateReviewStats(averageRating, reviewCount);
            await _catalogContext.SaveChangesAsync(cancellationToken);
        }
    }
}
