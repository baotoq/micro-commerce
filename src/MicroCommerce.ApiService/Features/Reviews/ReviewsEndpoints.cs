using System.Security.Claims;
using MediatR;
using MicroCommerce.ApiService.Features.Reviews.Application.Commands.CreateReview;
using MicroCommerce.ApiService.Features.Reviews.Application.Commands.DeleteReview;
using MicroCommerce.ApiService.Features.Reviews.Application.Commands.UpdateReview;
using MicroCommerce.ApiService.Features.Reviews.Application.Queries.CheckUserPurchased;
using MicroCommerce.ApiService.Features.Reviews.Application.Queries.GetReviewsByProduct;
using MicroCommerce.ApiService.Features.Reviews.Application.Queries.GetUserReviewForProduct;

namespace MicroCommerce.ApiService.Features.Reviews;

/// <summary>
/// Reviews module endpoints.
/// Provides product review management including create, update, delete, and list reviews.
/// </summary>
public static class ReviewsEndpoints
{
    public static IEndpointRouteBuilder MapReviewsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reviews")
            .WithTags("Reviews");

        // Public endpoints (no auth required for reading)
        group.MapGet("/products/{productId:guid}", GetProductReviews)
            .WithName("GetProductReviews")
            .WithSummary("Get reviews for a product")
            .Produces<ReviewListDto>();

        // Authenticated endpoints
        group.MapGet("/products/{productId:guid}/mine", GetMyReview)
            .WithName("GetMyReview")
            .WithSummary("Get current user's review for a product")
            .RequireAuthorization()
            .Produces<ReviewDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/products/{productId:guid}/can-review", CanReview)
            .WithName("CanReview")
            .WithSummary("Check if user can review a product")
            .RequireAuthorization()
            .Produces<CanReviewDto>();

        group.MapPost("/products/{productId:guid}", CreateReview)
            .WithName("CreateReview")
            .WithSummary("Submit a review for a purchased product")
            .RequireAuthorization()
            .Produces<CreateReviewResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{reviewId:guid}", UpdateReview)
            .WithName("UpdateReview")
            .WithSummary("Update your review")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{reviewId:guid}", DeleteReview)
            .WithName("DeleteReview")
            .WithSummary("Delete your review")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> GetProductReviews(
        Guid productId,
        int page,
        int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        // Apply defaults
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 5;

        var result = await sender.Send(
            new GetReviewsByProductQuery(productId, page, pageSize),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetMyReview(
        Guid productId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        var result = await sender.Send(
            new GetUserReviewForProductQuery(userId, productId),
            cancellationToken);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> CanReview(
        Guid productId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        var hasPurchased = await sender.Send(
            new CheckUserPurchasedQuery(userId, productId),
            cancellationToken);

        var existingReview = await sender.Send(
            new GetUserReviewForProductQuery(userId, productId),
            cancellationToken);

        var result = new CanReviewDto(hasPurchased, existingReview is not null);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateReview(
        Guid productId,
        CreateReviewRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        var reviewId = await sender.Send(
            new CreateReviewCommand(userId, productId, request.Rating, request.Text),
            cancellationToken);

        return Results.Created($"/api/reviews/{reviewId}", new CreateReviewResult(reviewId));
    }

    private static async Task<IResult> UpdateReview(
        Guid reviewId,
        UpdateReviewRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        await sender.Send(
            new UpdateReviewCommand(userId, reviewId, request.Rating, request.Text),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteReview(
        Guid reviewId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        await sender.Send(
            new DeleteReviewCommand(userId, reviewId),
            cancellationToken);

        return Results.NoContent();
    }

    private static Guid GetUserId(HttpContext context)
    {
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }

        return userId;
    }
}

// Request/response records for endpoint contracts
public sealed record CreateReviewRequest(int Rating, string Text);

public sealed record UpdateReviewRequest(int Rating, string Text);

public sealed record CreateReviewResult(Guid Id);

public sealed record CanReviewDto(bool HasPurchased, bool HasReviewed);
