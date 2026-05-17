using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;

namespace MicroCommerce.Catalog.Api.EventHandlers;

class ProductCreatedEventHandler(DaprClient dapr) : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent e, CancellationToken ct)
        => dapr.PublishEventAsync("pubsub", "catalog.product.created", e, ct);
}
