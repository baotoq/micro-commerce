using Ardalis.GuardClauses;
using MicroCommerce.BuildingBlocks.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.BuildingBlocks.Common;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the in-process MediatR-based domain event dispatcher.
    /// </summary>
    /// <remarks>
    /// OBSOLETE: Use MassTransit with transactional outbox instead.
    ///
    /// Migration guide:
    /// 1. Remove this call from your DI configuration
    /// 2. Configure MassTransit with EF Core outbox in Program.cs
    /// 3. Add DomainEventInterceptor to your DbContext
    /// 4. Domain events will be published via Azure Service Bus
    /// </remarks>
    [Obsolete("Use MassTransit with transactional outbox pattern. Configure MassTransit in Program.cs and use DomainEventInterceptor.")]
#pragma warning disable CS0618 // Intentionally using obsolete types in obsolete method
    public static void AddMediatorDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
    }
#pragma warning restore CS0618
}
