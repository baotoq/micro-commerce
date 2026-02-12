using MediatR;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Reviews.Application.Queries.GetReviewsByProduct;

public sealed record GetReviewsByProductQuery(
    Guid ProductId,
    int Page = 1,
    int PageSize = 5) : IRequest<ReviewListDto>;

public sealed record ReviewDto(
    Guid Id,
    Guid UserId,
    string DisplayName,
    int Rating,
    string Text,
    DateTimeOffset CreatedAt,
    bool IsVerifiedPurchase);

public sealed record ReviewListDto(
    List<ReviewDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed class GetReviewsByProductQueryHandler
    : IRequestHandler<GetReviewsByProductQuery, ReviewListDto>
{
    private readonly ReviewsDbContext _reviewsContext;
    private readonly ProfilesDbContext _profilesContext;
    private readonly OrderingDbContext _orderingContext;

    public GetReviewsByProductQueryHandler(
        ReviewsDbContext reviewsContext,
        ProfilesDbContext profilesContext,
        OrderingDbContext orderingContext)
    {
        _reviewsContext = reviewsContext;
        _profilesContext = profilesContext;
        _orderingContext = orderingContext;
    }

    public async Task<ReviewListDto> Handle(
        GetReviewsByProductQuery request,
        CancellationToken cancellationToken)
    {
        // Get total count for pagination
        var totalCount = await _reviewsContext.Reviews
            .Where(r => r.ProductId == request.ProductId)
            .CountAsync(cancellationToken);

        // Query reviews with pagination, sorted by newest first
        var reviews = await _reviewsContext.Reviews
            .Where(r => r.ProductId == request.ProductId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
        {
            return new ReviewListDto([], totalCount, request.Page, request.PageSize);
        }

        // Batch lookup: Get user IDs
        var userIds = reviews.Select(r => r.UserId).Distinct().ToList();

        // Batch lookup: Get display names from profiles
        var profiles = await _profilesContext.UserProfiles
            .Where(p => userIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, p => p.DisplayName.Value, cancellationToken);

        // Batch lookup: Check verified purchases
        var verifiedUserIds = await _orderingContext.Orders
            .Where(o => userIds.Contains(o.BuyerId))
            .Where(o => o.Status == OrderStatus.Paid ||
                        o.Status == OrderStatus.Confirmed ||
                        o.Status == OrderStatus.Shipped ||
                        o.Status == OrderStatus.Delivered)
            .Where(o => o.Items.Any(item => item.ProductId == request.ProductId))
            .Select(o => o.BuyerId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var verifiedSet = verifiedUserIds.ToHashSet();

        // Map to DTOs
        var items = reviews.Select(r => new ReviewDto(
            r.Id.Value,
            r.UserId,
            profiles.GetValueOrDefault(r.UserId, "User"),
            r.Rating.Value,
            r.Text.Value,
            r.CreatedAt,
            verifiedSet.Contains(r.UserId)
        )).ToList();

        return new ReviewListDto(items, totalCount, request.Page, request.PageSize);
    }
}
