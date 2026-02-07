using System.Text.Json;
using MassTransit;
using MicroCommerce.ApiService.Features.Inventory.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Consumers;

/// <summary>
/// Permanently deducts stock for a confirmed order.
/// Adjusts QuantityOnHand (negative) matching the reserved quantity, then releases the reservation.
/// </summary>
public sealed class DeductStockConsumer(
    InventoryDbContext inventoryDb,
    ILogger<DeductStockConsumer> logger) : IConsumer<DeductStock>
{
    public async Task Consume(ConsumeContext<DeductStock> context)
    {
        Dictionary<Guid, Guid>? reservationMap = JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(
            context.Message.ReservationIdsJson);

        if (reservationMap is null || reservationMap.Count == 0)
        {
            logger.LogWarning("DeductStock for order {OrderId} has empty reservation map", context.Message.OrderId);
            return;
        }

        foreach (KeyValuePair<Guid, Guid> entry in reservationMap)
        {
            Guid productId = entry.Key;
            Guid reservationId = entry.Value;

            Inventory.Domain.Entities.StockItem? stockItem = await inventoryDb.StockItems
                .Include(s => s.Reservations)
                .FirstOrDefaultAsync(s => s.ProductId == productId, context.CancellationToken);

            if (stockItem is null)
            {
                logger.LogWarning("StockItem not found for product {ProductId} during deduction", productId);
                continue;
            }

            // Find the reservation to get the quantity
            Inventory.Domain.Entities.StockReservation? reservation = stockItem.Reservations
                .FirstOrDefault(r => r.Id == ReservationId.From(reservationId));

            if (reservation is not null)
            {
                int quantity = reservation.Quantity;

                // Permanently deduct the stock
                stockItem.AdjustStock(-quantity, "Checkout order confirmed", "system");

                // Release the reservation (it's been converted to permanent deduction)
                stockItem.ReleaseReservation(ReservationId.From(reservationId));
            }
            else
            {
                logger.LogWarning(
                    "Reservation {ReservationId} not found for product {ProductId} during deduction (may have expired)",
                    reservationId, productId);
            }
        }

        await inventoryDb.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Stock deducted for order {OrderId}: {Count} items",
            context.Message.OrderId,
            reservationMap.Count);
    }
}
