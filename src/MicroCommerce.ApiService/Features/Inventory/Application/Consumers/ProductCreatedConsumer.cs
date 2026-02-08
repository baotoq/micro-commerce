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
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = context.CorrelationId?.ToString() ?? "none",
            ["MessageId"] = context.MessageId?.ToString() ?? "none",
            ["ConversationId"] = context.ConversationId?.ToString() ?? "none"
        });

        var productId = context.Message.ProductId;

        // Supplementary idempotency check - defense-in-depth alongside inbox deduplication
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
