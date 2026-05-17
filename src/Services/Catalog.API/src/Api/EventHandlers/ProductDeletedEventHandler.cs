using Dapr.Client;
using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;

namespace MicroCommerce.Catalog.Api.EventHandlers;

class ProductDeletedEventHandler(DaprClient dapr) : INotificationHandler<ProductDeletedEvent>
{
    public Task Handle(ProductDeletedEvent e, CancellationToken ct)
        => dapr.PublishEventAsync("pubsub", "catalog.product.deleted", e, ct);
}
