using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MicroCommerce.ApiService.Features.Inventory.Application.Commands.AdjustStock;
using MicroCommerce.ApiService.Features.Inventory.Application.Commands.ReleaseReservation;
using MicroCommerce.ApiService.Features.Inventory.Application.Commands.ReserveStock;
using MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetAdjustmentHistory;
using MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockByProductId;
using MicroCommerce.ApiService.Features.Inventory.Application.Queries.GetStockLevels;

namespace MicroCommerce.ApiService.Features.Inventory;

/// <summary>
/// Inventory module endpoints.
/// Provides stock management, reservation, and query operations.
/// </summary>
public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/inventory")
            .WithTags("Inventory");

        group.MapGet("/stock/{productId:guid}", GetStockByProductId)
            .WithName("GetStockByProductId")
            .WithSummary("Get stock info for a product")
            .Produces<StockInfoDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/stock", GetStockLevels)
            .WithName("GetStockLevels")
            .WithSummary("Get stock levels for multiple products")
            .Produces<List<StockInfoDto>>();

        group.MapPost("/stock/{productId:guid}/adjust", AdjustStock)
            .WithName("AdjustStock")
            .WithSummary("Adjust stock quantity for a product")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/stock/{productId:guid}/reserve", ReserveStock)
            .WithName("ReserveStock")
            .WithSummary("Reserve stock for a pending order")
            .Produces<ReserveStockResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/reservations/{reservationId:guid}", ReleaseReservation)
            .WithName("ReleaseReservation")
            .WithSummary("Release a stock reservation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/stock/{productId:guid}/adjustments", GetAdjustmentHistory)
            .WithName("GetAdjustmentHistory")
            .WithSummary("Get stock adjustment history for a product")
            .Produces<List<AdjustmentDto>>();

        return endpoints;
    }

    private static async Task<IResult> GetStockByProductId(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockByProductIdQuery(productId), cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetStockLevels(
        [FromQuery] string? productIds,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var ids = new List<Guid>();

        if (!string.IsNullOrWhiteSpace(productIds))
        {
            foreach (var idStr in productIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (Guid.TryParse(idStr, out var id))
                {
                    ids.Add(id);
                }
            }
        }

        var result = await sender.Send(new GetStockLevelsQuery(ids), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdjustStock(
        Guid productId,
        AdjustStockRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var adjustedBy = user.FindFirst("preferred_username")?.Value ?? "system";

        var command = new AdjustStockCommand(productId, request.Adjustment, request.Reason, adjustedBy);
        await sender.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ReserveStock(
        Guid productId,
        ReserveStockRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ReserveStockCommand(productId, request.Quantity);
        var reservationId = await sender.Send(command, cancellationToken);

        return Results.Created(
            $"/api/inventory/reservations/{reservationId}",
            new ReserveStockResponse(reservationId));
    }

    private static async Task<IResult> ReleaseReservation(
        Guid reservationId,
        [FromQuery] Guid stockItemId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ReleaseReservationCommand(stockItemId, reservationId);
        await sender.Send(command, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetAdjustmentHistory(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdjustmentHistoryQuery(productId), cancellationToken);
        return Results.Ok(result);
    }
}

// Request/Response records for endpoint contracts
public sealed record AdjustStockRequest(int Adjustment, string? Reason);

public sealed record ReserveStockRequest(int Quantity);

public sealed record ReserveStockResponse(Guid ReservationId);
