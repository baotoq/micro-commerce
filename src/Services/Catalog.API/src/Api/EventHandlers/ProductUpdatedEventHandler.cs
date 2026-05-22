using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.Api.EventHandlers;

// TODO(outbox): implement transactional outbox (audit#9, see audit/backend-followup.md).
class ProductUpdatedEventHandler(DaprClient dapr, ILogger<ProductUpdatedEventHandler> logger) : INotificationHandler<ProductUpdatedEvent>
{
    public async Task Handle(ProductUpdatedEvent e, CancellationToken ct)
    {
        try
        {
            await dapr.PublishEventAsync("pubsub", "catalog.product.updated", e, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish {Event} for SKU {Sku}", nameof(ProductUpdatedEvent), e.Sku);
        }
    }
}
