using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.BuildingBlocks.Common;

public static class DependencyInjection
{
    public static void AddMediatorDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
    }
}
