using MediatR;
using MicroCommerce.Catalog.Application.Payouts.Events;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.EventHandlers;

internal sealed class PayoutCreatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<PayoutCreatedEvent>
{
    public async Task Handle(PayoutCreatedEvent _, CancellationToken ct)
    {
        await cache.EvictByTagAsync("payouts", ct);
        await cache.EvictByTagAsync("dashboard", ct);
    }
}

internal sealed class LedgerEntryCreatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<LedgerEntryCreatedEvent>
{
    public async Task Handle(LedgerEntryCreatedEvent _, CancellationToken ct)
    {
        await cache.EvictByTagAsync("payouts", ct);
        await cache.EvictByTagAsync("dashboard", ct);
    }
}
