using MicroCommerce.BuildingBlocks.Common;
using MicroCommerce.BuildingBlocks.Common.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MicroCommerce.BuildingBlocks.EFCore.Interceptors;

public class DispatchDomainEventsInterceptor(IDomainEventDispatcher domainEventDispatcher) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var r = base.SavingChanges(eventData, result);
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
        return r;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var r = await base.SavingChangesAsync(eventData, result, cancellationToken);
        await DispatchDomainEvents(eventData.Context);
        return r;
    }

    private async Task DispatchDomainEvents(DbContext? context)
    {
        if (context is null) return;

        var entities = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        if (domainEvents.Count != 0)
        {
            await domainEventDispatcher.DispatchAsync(domainEvents);
        }

        entities.ForEach(e => e.ClearDomainEvents());
    }
}
