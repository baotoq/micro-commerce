using MediatR;
using MicroCommerce.ApiService.Features.Cart.Application.Commands.AddToCart;
using MicroCommerce.ApiService.Features.Cart.Application.Commands.RemoveCartItem;
using MicroCommerce.ApiService.Features.Cart.Application.Commands.UpdateCartItem;
using MicroCommerce.ApiService.Features.Cart.Application.Queries.GetCart;
using MicroCommerce.ApiService.Features.Cart.Application.Queries.GetCartItemCount;

namespace MicroCommerce.ApiService.Features.Cart;

/// <summary>
/// Cart module endpoints.
/// Provides cart management for both guest and authenticated buyers via cookie-based identity.
/// </summary>
public static class CartEndpoints
{
    public static IEndpointRouteBuilder MapCartEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/cart")
            .WithTags("Cart");

        group.MapGet("/", GetCart)
            .WithName("GetCart")
            .WithSummary("Get cart for current buyer")
            .Produces<CartDto>()
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/items", AddToCart)
            .WithName("AddToCart")
            .WithSummary("Add item to cart (creates cart if needed)")
            .Produces<AddToCartResult>()
            .ProducesValidationProblem();

        group.MapPut("/items/{itemId:guid}", UpdateCartItem)
            .WithName("UpdateCartItem")
            .WithSummary("Update cart item quantity")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/items/{itemId:guid}", RemoveCartItem)
            .WithName("RemoveCartItem")
            .WithSummary("Remove item from cart")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/count", GetCartItemCount)
            .WithName("GetCartItemCount")
            .WithSummary("Get total item count for cart badge")
            .Produces<int>();

        return endpoints;
    }

    private static async Task<IResult> GetCart(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);
        var result = await sender.Send(new GetCartQuery(buyerId), cancellationToken);
        return result is null ? Results.NoContent() : Results.Ok(result);
    }

    private static async Task<IResult> AddToCart(
        AddToCartRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);

        var command = new AddToCartCommand(
            buyerId,
            request.ProductId,
            request.ProductName,
            request.UnitPrice,
            request.ImageUrl,
            request.Quantity);

        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateCartItem(
        Guid itemId,
        UpdateCartItemRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);

        var command = new UpdateCartItemCommand(buyerId, itemId, request.Quantity);
        await sender.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveCartItem(
        Guid itemId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);

        var command = new RemoveCartItemCommand(buyerId, itemId);
        await sender.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetCartItemCount(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var buyerId = BuyerIdentity.GetOrCreateBuyerId(httpContext);
        var count = await sender.Send(new GetCartItemCountQuery(buyerId), cancellationToken);
        return Results.Ok(count);
    }
}

// Request records for endpoint contracts
public sealed record AddToCartRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string? ImageUrl,
    int Quantity);

public sealed record UpdateCartItemRequest(int Quantity);
