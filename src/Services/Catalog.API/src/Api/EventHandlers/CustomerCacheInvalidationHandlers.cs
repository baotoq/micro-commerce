using MediatR;
using MicroCommerce.Catalog.Application.Customers.Events;
using Microsoft.AspNetCore.OutputCaching;

namespace MicroCommerce.Catalog.Api.EventHandlers;

// D6 cache-eviction matrix: a customer mutation invalidates the customers list and any
// order views that surface customer tags / lifetime stats joined by CustomerEmail.
internal sealed class CustomerCreatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<CustomerCreatedEvent>
{
    public async Task Handle(CustomerCreatedEvent _, CancellationToken ct)
    {
        await cache.EvictByTagAsync("customers", ct);
        await cache.EvictByTagAsync("orders", ct);
    }
}

internal sealed class CustomerUpdatedCacheInvalidator(IOutputCacheStore cache) : INotificationHandler<CustomerUpdatedEvent>
{
    public async Task Handle(CustomerUpdatedEvent _, CancellationToken ct)
    {
        await cache.EvictByTagAsync("customers", ct);
        await cache.EvictByTagAsync("orders", ct);
    }
}
