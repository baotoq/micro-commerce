using MicroCommerce.ApiService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MicroCommerce.ApiService.Infrastructure.Interceptors;

public class IndexProductInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public IndexProductInterceptor(IDomainEventDispatcher domainEventDispatcher)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var r = base.SavingChanges(eventData, result);
        
        DispatchEvents(eventData.Context).GetAwaiter().GetResult();

        return r;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var r = await base.SavingChangesAsync(eventData, result, cancellationToken);
        
        await DispatchEvents(eventData.Context);

        return r;
    }
    
    private async Task DispatchEvents(DbContext? context)
    {
        if (context is null) return;

        var events = new List<IndexProductDomainEvent>();
        foreach (var entry in context.ChangeTracker.Entries<Product>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                case EntityState.Deleted:
                    events.Add(new IndexProductDomainEvent
                    {
                        ProductId = entry.Entity.Id
                    });
                    break;
            }
        }
        
        await _domainEventDispatcher.DispatchAsync(events);
    }
}