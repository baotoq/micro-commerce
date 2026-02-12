using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Features.Reviews.Domain.Entities;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Reviews.Application.Commands.CreateReview;

public sealed record CreateReviewCommand(
    Guid UserId,
    Guid ProductId,
    int Rating,
    string Text) : IRequest<Guid>;

public sealed class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Guid>
{
    private readonly ReviewsDbContext _reviewsContext;
    private readonly OrderingDbContext _orderingContext;
    private readonly CatalogDbContext _catalogContext;

    public CreateReviewCommandHandler(
        ReviewsDbContext reviewsContext,
        OrderingDbContext orderingContext,
        CatalogDbContext catalogContext)
    {
        _reviewsContext = reviewsContext;
        _orderingContext = orderingContext;
        _catalogContext = catalogContext;
    }

    public async Task<Guid> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Verify purchase - user must have purchased this product
        var hasPurchased = await _orderingContext.Orders
            .Where(o => o.BuyerId == request.UserId)
            .Where(o => o.Status == OrderStatus.Paid ||
                        o.Status == OrderStatus.Confirmed ||
                        o.Status == OrderStatus.Shipped ||
                        o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.Items)
            .AnyAsync(item => item.ProductId == request.ProductId, cancellationToken);

        if (!hasPurchased)
        {
            throw new InvalidOperationException("You must purchase this product before leaving a review.");
        }

        // Step 2: Create review via factory method
        var review = Review.Create(
            request.ProductId,
            request.UserId,
            request.Rating,
            request.Text);

        // Step 3: Add to context and save - catch unique constraint violation
        _reviewsContext.Reviews.Add(review);

        try
        {
            await _reviewsContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true ||
                                            ex.InnerException?.Message.Contains("unique constraint") == true)
        {
            throw new ConflictException("You have already reviewed this product.");
        }

        // Step 4: Recalculate aggregate ratings
        await RecalculateProductRatings(request.ProductId, cancellationToken);

        return review.Id.Value;
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
