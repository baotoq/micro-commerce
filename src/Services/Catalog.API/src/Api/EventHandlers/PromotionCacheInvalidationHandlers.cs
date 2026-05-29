using MediatR;
using MicroCommerce.Catalog.Application.Promotions.Events;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.EventHandlers;

internal sealed class PromotionCreatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<PromotionCreatedEvent>
{
    public Task Handle(PromotionCreatedEvent _, CancellationToken ct) => cache.EvictByTagAsync("promotions", ct).AsTask();
}

internal sealed class PromotionUpdatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<PromotionUpdatedEvent>
{
    public Task Handle(PromotionUpdatedEvent _, CancellationToken ct) => cache.EvictByTagAsync("promotions", ct).AsTask();
}

internal sealed class PromotionDeletedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<PromotionDeletedEvent>
{
    public Task Handle(PromotionDeletedEvent _, CancellationToken ct) => cache.EvictByTagAsync("promotions", ct).AsTask();
}

internal sealed class PromotionStatusChangedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<PromotionStatusChangedEvent>
{
    public Task Handle(PromotionStatusChangedEvent _, CancellationToken ct) => cache.EvictByTagAsync("promotions", ct).AsTask();
}
