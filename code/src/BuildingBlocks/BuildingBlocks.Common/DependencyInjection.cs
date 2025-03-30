using Ardalis.GuardClauses;
using MicroCommerce.BuildingBlocks.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.BuildingBlocks.Common;

public static class DependencyInjection
{
    public static void AddMediatorDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
    }
}
