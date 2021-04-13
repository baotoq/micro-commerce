using Dapr.Client;
using MicroCommerce.Shared.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Shared.EventBus
{
    public static class EventBusDependencyInjection
    {
        public static void AddDaprEvenBus(this IServiceCollection services, string pubsubName = "pubsub")
        {
            services.AddScoped<IEventBus>(resolver =>
                new DaprEventBus(pubsubName, resolver.GetRequiredService<DaprClient>(), resolver.GetRequiredService<ILogger<DaprEventBus>>()));
        }
    }
}
