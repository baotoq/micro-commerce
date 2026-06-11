using MediatR;
using MicroCommerce.Catalog.Application.Orders.Events;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.EventHandlers;

// D6 cache eviction matrix: an Order mutation (Created/StatusChanged/Updated) ripples into
// payouts (net-of-new derivation), analytics (revenue/top-products), the dashboard
// (pack-ready count), and customer lifetime stats — so all five tags are evicted.
internal sealed class OrderCreatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent _, CancellationToken ct)
    {
        await cache.EvictByTagAsync("orders", ct).AsTask();
        await cache.EvictByTagAsync("payouts", ct).AsTask();
        await cache.EvictByTagAsync("analytics", ct).AsTask();
        await cache.EvictByTagAsync("dashboard", ct).AsTask();
        await cache.EvictByTagAsync("customers", ct).AsTask();
    }
}

internal sealed class OrderStatusChangedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<OrderStatusChangedEvent>
{
    public async Task Handle(OrderStatusChangedEvent _, CancellationToken ct)
    {
        await cache.EvictByTagAsync("orders", ct).AsTask();
        await cache.EvictByTagAsync("payouts", ct).AsTask();
        await cache.EvictByTagAsync("analytics", ct).AsTask();
        await cache.EvictByTagAsync("dashboard", ct).AsTask();
        await cache.EvictByTagAsync("customers", ct).AsTask();
    }
}

internal sealed class OrderUpdatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<OrderUpdatedEvent>
{
    public async Task Handle(OrderUpdatedEvent _, CancellationToken ct)
    {
        await cache.EvictByTagAsync("orders", ct).AsTask();
        await cache.EvictByTagAsync("payouts", ct).AsTask();
        await cache.EvictByTagAsync("analytics", ct).AsTask();
        await cache.EvictByTagAsync("dashboard", ct).AsTask();
        await cache.EvictByTagAsync("customers", ct).AsTask();
    }
}
