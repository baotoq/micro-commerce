using MediatR;
using MicroCommerce.Catalog.Application.Products.Events;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.EventHandlers;

// Invalidate the "products" output-cache tag after every product mutation (audit#8).
// Kept separate from the Dapr publish handlers so a Dapr publish failure does not block
// eviction (and vice versa).
internal sealed class ProductCreatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent _, CancellationToken ct) => cache.EvictByTagAsync("products", ct).AsTask();
}

internal sealed class ProductUpdatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<ProductUpdatedEvent>
{
    public Task Handle(ProductUpdatedEvent _, CancellationToken ct) => cache.EvictByTagAsync("products", ct).AsTask();
}

internal sealed class ProductDeletedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<ProductDeletedEvent>
{
    public Task Handle(ProductDeletedEvent _, CancellationToken ct) => cache.EvictByTagAsync("products", ct).AsTask();
}
