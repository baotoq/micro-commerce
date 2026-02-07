using System.Text.Json;
using MassTransit;
using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Consumers;

/// <summary>
/// Compensation handler: releases all stock reservations when payment fails.
/// Idempotent - no-op if reservations are already released or expired.
/// </summary>
public sealed class ReleaseStockReservationsConsumer(
    InventoryDbContext inventoryDb,
    ILogger<ReleaseStockReservationsConsumer> logger) : IConsumer<ReleaseStockReservations>
{
    public async Task Consume(ConsumeContext<ReleaseStockReservations> context)
    {
        if (string.IsNullOrEmpty(context.Message.ReservationIdsJson))
        {
            logger.LogWarning(
                "ReleaseStockReservations for order {OrderId} has empty reservation map",
                context.Message.OrderId);
            return;
        }

        Dictionary<Guid, Guid>? reservationMap = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(
            context.Message.ReservationIdsJson);

        if (reservationMap is null || reservationMap.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<Guid, Guid> entry in reservationMap)
        {
            Guid productId = entry.Key;
            Guid reservationId = entry.Value;

            Inventory.Domain.Entities.StockItem? stockItem = await inventoryDb.StockItems
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(s => s.ProductId == productId, context.CancellationToken);

            if (stockItem is not null)
            {
                stockItem.ReleaseReservation(ReservationId.From(reservationId));
            }
        }

        await inventoryDb.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Stock reservations released for order {OrderId}: {Count} items (compensation)",
            context.Message.OrderId,
            reservationMap.Count);
    }
}
