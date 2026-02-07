using System.Text.Json;
using MassTransit;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Consumers;

/// <summary>
/// Handles stock reservation for all items in a checkout order.
/// On success: publishes StockReservationCompleted with reservation IDs.
/// On failure: releases any partial reservations and publishes StockReservationFailed.
/// </summary>
public sealed class ReserveStockForOrderConsumer(
    InventoryDbContext inventoryDb,
    ILogger<ReserveStockForOrderConsumer> logger) : IConsumer<ReserveStockForOrder>
{
    public async Task Consume(ConsumeContext<ReserveStockForOrder> context)
    {
        Dictionary<Guid, Guid> reservationMap = [];
        List<(Guid StockItemId, Guid ReservationId)> completedReservations = [];

        try
        {
            foreach (CheckoutItem item in context.Message.Items)
            {
                Inventory.Domain.Entities.StockItem? stockItem = await inventoryDb.StockItems
                    .Include(s => s.Reservations)
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId, context.CancellationToken);

                if (stockItem is null)
                {
                    throw new InvalidOperationException($"No stock item found for product {item.ProductId}");
                }

                Inventory.Domain.ValueObjects.ReservationId reservationId = stockItem.Reserve(item.Quantity);
                reservationMap[item.ProductId] = reservationId.Value;
                completedReservations.Add((stockItem.Id.Value, reservationId.Value));
            }

            await inventoryDb.SaveChangesAsync(context.CancellationToken);

            string reservationIdsJson = JsonSerializer.Serialize(reservationMap);

            logger.LogInformation(
                "Stock reserved for order {OrderId}: {ReservationCount} items",
                context.Message.OrderId,
                reservationMap.Count);

            await context.Publish<StockReservationCompleted>(new
            {
                context.Message.OrderId,
                ReservationIdsJson = reservationIdsJson
            }, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Stock reservation failed for order {OrderId}: {Reason}",
                context.Message.OrderId,
                ex.Message);

            // Compensation: release any reservations that were already made
            foreach ((Guid stockItemId, Guid reservationId) in completedReservations)
            {
                try
                {
                    Inventory.Domain.Entities.StockItem? stockItem = await inventoryDb.StockItems
                        .Include(s => s.Reservations)
                        .FirstOrDefaultAsync(
                            s => s.Id == Inventory.Domain.ValueObjects.StockItemId.From(stockItemId),
                            context.CancellationToken);

                    stockItem?.ReleaseReservation(Inventory.Domain.ValueObjects.ReservationId.From(reservationId));
                }
                catch (Exception releaseEx)
                {
                    logger.LogError(
                        releaseEx,
                        "Failed to release reservation {ReservationId} during compensation",
                        reservationId);
                }
            }

            await inventoryDb.SaveChangesAsync(context.CancellationToken);

            await context.Publish<StockReservationFailed>(new
            {
                context.Message.OrderId,
                Reason = ex.Message
            }, context.CancellationToken);
        }
    }
}
