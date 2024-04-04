using Domain.Common;

namespace Infrastructure.Interceptors;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents);
}