using MediatR;
using MicroCommerce.Catalog.Application.Marketing.Events;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.EventHandlers;

internal sealed class CampaignCreatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<CampaignCreatedEvent>
{
    public Task Handle(CampaignCreatedEvent _, CancellationToken ct) => cache.EvictByTagAsync("marketing", ct).AsTask();
}

internal sealed class CampaignUpdatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<CampaignUpdatedEvent>
{
    public Task Handle(CampaignUpdatedEvent _, CancellationToken ct) => cache.EvictByTagAsync("marketing", ct).AsTask();
}
