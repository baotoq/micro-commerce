using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Reviews.Application.Commands.UpdateReview;

public sealed record UpdateReviewCommand(
    Guid UserId,
    Guid ReviewId,
    int Rating,
    string Text) : IRequest;

public sealed class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand>
{
    private readonly ReviewsDbContext _reviewsContext;
    private readonly CatalogDbContext _catalogContext;

    public UpdateReviewCommandHandler(
        ReviewsDbContext reviewsContext,
        CatalogDbContext catalogContext)
    {
        _reviewsContext = reviewsContext;
        _catalogContext = catalogContext;
    }

    public async Task Handle(
        UpdateReviewCommand request,
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
            throw new UnauthorizedAccessException("You can only update your own reviews.");
        }

        // Store ProductId for aggregate recalculation
        var productId = review.ProductId;

        // Update review
        review.Update(request.Rating, request.Text);
        await _reviewsContext.SaveChangesAsync(cancellationToken);

        // Recalculate aggregate ratings
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

        // Load product and update stats
        var product = await _catalogContext.Products
            .FirstOrDefaultAsync(p => p.Id == new ProductId(productId), cancellationToken);

        if (product is not null)
        {
            product.UpdateReviewStats(averageRating, reviewCount);
            await _catalogContext.SaveChangesAsync(cancellationToken);
        }
    }
}
