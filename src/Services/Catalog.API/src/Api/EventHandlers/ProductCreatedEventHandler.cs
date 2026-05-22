using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Catalog.Api.EventHandlers;

// TODO(outbox): implement transactional outbox so the event row is committed in the same
// transaction as the Product write, and a background dispatcher publishes via Dapr with
// at-least-once retry. See audit/backend-followup.md (audit#9).
class ProductCreatedEventHandler(DaprClient dapr, ILogger<ProductCreatedEventHandler> logger) : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent e, CancellationToken ct)
    {
        try
        {
            await dapr.PublishEventAsync("pubsub", "catalog.product.created", e, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish {Event} for SKU {Sku}", nameof(ProductCreatedEvent), e.Sku);
        }
    }
}
