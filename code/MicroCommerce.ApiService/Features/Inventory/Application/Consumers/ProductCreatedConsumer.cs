using MassTransit;
using MicroCommerce.ApiService.Features.Catalog.Domain.Events;
using MicroCommerce.ApiService.Features.Inventory.Domain.Entities;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Consumers;

/// <summary>
/// MassTransit consumer that auto-creates a StockItem when a new product is created.
/// Idempotent - skips if StockItem already exists for the product.
/// </summary>
public sealed class ProductCreatedConsumer : IConsumer<ProductCreatedDomainEvent>
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(InventoryDbContext context, ILogger<ProductCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedDomainEvent> context)
    {
        var productId = context.Message.ProductId;

        // Idempotency check - skip if stock item already exists
        var exists = await _context.StockItems
            .AnyAsync(s => s.ProductId == productId, context.CancellationToken);

        if (exists)
        {
            _logger.LogInformation("StockItem already exists for product {ProductId}, skipping.", productId);
            return;
        }

        var stockItem = StockItem.Create(productId);
        _context.StockItems.Add(stockItem);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Created StockItem {StockItemId} for product {ProductId}.", stockItem.Id.Value, productId);
    }
}
