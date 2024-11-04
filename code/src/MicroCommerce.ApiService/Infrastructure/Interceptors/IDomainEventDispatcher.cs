using MicroCommerce.ApiService.Domain.Common;

namespace MicroCommerce.ApiService.Infrastructure.Interceptors;

public interface IDomainEventDispatcher
{
    public Task DispatchAsync<T>(IEnumerable<T> domainEvents) where T : IDomainEvent;
    public Task DispatchAsync<T>(IDomainEvent domainEvent) where T : IDomainEvent;
}