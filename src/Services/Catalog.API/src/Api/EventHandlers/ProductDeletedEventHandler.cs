using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.Api.EventHandlers;

// TODO(outbox): implement transactional outbox (audit#9, see audit/backend-followup.md).
class ProductDeletedEventHandler(DaprClient dapr, ILogger<ProductDeletedEventHandler> logger) : INotificationHandler<ProductDeletedEvent>
{
    public async Task Handle(ProductDeletedEvent e, CancellationToken ct)
    {
        try
        {
            await dapr.PublishEventAsync("pubsub", "catalog.product.deleted", e, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish {Event} for SKU {Sku}", nameof(ProductDeletedEvent), e.Sku);
        }
    }
}
