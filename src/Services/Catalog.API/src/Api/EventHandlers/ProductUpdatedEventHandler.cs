using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;

namespace MicroCommerce.Catalog.Api.EventHandlers;

class ProductUpdatedEventHandler(DaprClient dapr) : INotificationHandler<ProductUpdatedEvent>
{
    public Task Handle(ProductUpdatedEvent e, CancellationToken ct)
        => dapr.PublishEventAsync("pubsub", "catalog.product.updated", e, ct);
}
