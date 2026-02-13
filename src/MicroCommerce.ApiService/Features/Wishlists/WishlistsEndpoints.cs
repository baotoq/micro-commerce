using System.Security.Claims;
using MediatR;
using MicroCommerce.ApiService.Features.Wishlists.Application.Commands.AddToWishlist;
using MicroCommerce.ApiService.Features.Wishlists.Application.Commands.RemoveFromWishlist;
using MicroCommerce.ApiService.Features.Wishlists.Application.Queries.GetUserWishlist;
using MicroCommerce.ApiService.Features.Wishlists.Application.Queries.GetWishlistCount;
using MicroCommerce.ApiService.Features.Wishlists.Application.Queries.GetWishlistProductIds;

namespace MicroCommerce.ApiService.Features.Wishlists;

/// <summary>
/// Wishlists module endpoints.
/// Provides wishlist management including add, remove, and list operations.
/// All endpoints require authentication.
/// </summary>
public static class WishlistsEndpoints
{
    public static IEndpointRouteBuilder MapWishlistsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/wishlist")
            .WithTags("Wishlist")
            .RequireAuthorization();

        // GET /api/wishlist - Get current user's wishlist with product details
        group.MapGet("/", GetUserWishlist)
            .WithName("GetUserWishlist")
            .WithSummary("Get current user's wishlist with product details")
            .Produces<List<WishlistItemDto>>();

        // GET /api/wishlist/count - Get wishlist item count
        group.MapGet("/count", GetWishlistCount)
            .WithName("GetWishlistCount")
            .WithSummary("Get wishlist item count")
            .Produces<int>();

        // GET /api/wishlist/product-ids - Get list of wishlisted product IDs
        group.MapGet("/product-ids", GetWishlistProductIds)
            .WithName("GetWishlistProductIds")
            .WithSummary("Get list of wishlisted product IDs")
            .Produces<List<Guid>>();

        // POST /api/wishlist/{productId} - Add product to wishlist
        group.MapPost("/{productId:guid}", AddToWishlist)
            .WithName("AddToWishlist")
            .WithSummary("Add product to wishlist")
            .Produces(StatusCodes.Status201Created);

        // DELETE /api/wishlist/{productId} - Remove product from wishlist
        group.MapDelete("/{productId:guid}", RemoveFromWishlist)
            .WithName("RemoveFromWishlist")
            .WithSummary("Remove product from wishlist")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static async Task<IResult> GetUserWishlist(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        var result = await sender.Send(
            new GetUserWishlistQuery(userId),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetWishlistCount(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        var count = await sender.Send(
            new GetWishlistCountQuery(userId),
            cancellationToken);

        return Results.Ok(count);
    }

    private static async Task<IResult> GetWishlistProductIds(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        var productIds = await sender.Send(
            new GetWishlistProductIdsQuery(userId),
            cancellationToken);

        return Results.Ok(productIds);
    }

    private static async Task<IResult> AddToWishlist(
        Guid productId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        var id = await sender.Send(
            new AddToWishlistCommand(userId, productId),
            cancellationToken);

        return Results.Created($"/api/wishlist", new { id });
    }

    private static async Task<IResult> RemoveFromWishlist(
        Guid productId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(httpContext);

        await sender.Send(
            new RemoveFromWishlistCommand(userId, productId),
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
