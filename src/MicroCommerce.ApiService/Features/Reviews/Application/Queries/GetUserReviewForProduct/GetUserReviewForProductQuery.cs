using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using MicroCommerce.ApiService.Features.Reviews.Application.Queries.GetReviewsByProduct;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Reviews.Application.Queries.GetUserReviewForProduct;

public sealed record GetUserReviewForProductQuery(
    Guid UserId,
    Guid ProductId) : IRequest<ReviewDto?>;

public sealed class GetUserReviewForProductQueryHandler
    : IRequestHandler<GetUserReviewForProductQuery, ReviewDto?>
{
    private readonly ReviewsDbContext _reviewsContext;
    private readonly ProfilesDbContext _profilesContext;
    private readonly OrderingDbContext _orderingContext;

    public GetUserReviewForProductQueryHandler(
        ReviewsDbContext reviewsContext,
        ProfilesDbContext profilesContext,
        OrderingDbContext orderingContext)
    {
        _reviewsContext = reviewsContext;
        _profilesContext = profilesContext;
        _orderingContext = orderingContext;
    }

    public async Task<ReviewDto?> Handle(
        GetUserReviewForProductQuery request,
        CancellationToken cancellationToken)
    {
        // Query review where UserId and ProductId match
        var review = await _reviewsContext.Reviews
            .FirstOrDefaultAsync(
                r => r.UserId == request.UserId && r.ProductId == request.ProductId,
                cancellationToken);

        if (review is null)
        {
            return null;
        }

        // Get display name
        var profile = await _profilesContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        var displayName = profile?.DisplayName.Value ?? "User";

        // Check if verified purchase
        var isVerifiedPurchase = await _orderingContext.Orders
            .Where(o => o.BuyerId == request.UserId)
            .Where(o => o.Status == OrderStatus.Paid ||
                        o.Status == OrderStatus.Confirmed ||
                        o.Status == OrderStatus.Shipped ||
                        o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.Items)
            .AnyAsync(item => item.ProductId == request.ProductId, cancellationToken);

        return new ReviewDto(
            review.Id.Value,
            review.UserId,
            displayName,
            review.Rating.Value,
            review.Text.Value,
            review.CreatedAt,
            isVerifiedPurchase);
    }
}
