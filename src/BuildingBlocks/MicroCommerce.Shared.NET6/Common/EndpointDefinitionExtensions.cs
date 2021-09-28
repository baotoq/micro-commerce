using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Shared.Common;

public static class EndpointDefinitionExtensions
{
    public static void AddEndpointDefinitions(this IServiceCollection services, params Type[] scanMarkers)
    {
        var endpointDefinitions = new List<IEndpointDefinition>();

        foreach (var item in scanMarkers)
        {
            endpointDefinitions.AddRange(
                    item.Assembly.ExportedTypes
                        .Where(s => typeof(IEndpointDefinition).IsAssignableFrom(s) && !s.IsAbstract)
                        .Select(Activator.CreateInstance).Cast<IEndpointDefinition>()
                );
        }

        endpointDefinitions.ForEach(s => s.DefineServices(services));
        services.AddSingleton(endpointDefinitions as IReadOnlyCollection<IEndpointDefinition>);
    }

    public static void UseEndpointDefinitions(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointDefinition>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.DefineEnpoints(app);
        }
    }

    public static IServiceCollection AddEndpointDefinition<TEndpoint>(this IServiceCollection services) where TEndpoint : class
    {
        if (!typeof(TEndpoint).IsAssignableTo(typeof(IEndpointDefinition)))
        {
            throw new ArgumentException($"{nameof(TEndpoint)} must be implemented {nameof(IEndpointDefinition)}");
        }

        var endpoint = Activator.CreateInstance(typeof(TEndpoint)) as IEndpointDefinition;

        endpoint!.DefineServices(services);

        services.AddSingleton<TEndpoint>();

        return services;
    }

    public static WebApplication UseEndpointDefinition<TEndpoint>(this WebApplication app) where TEndpoint : IEndpointDefinition
    {
        var endpoint = app.Services.GetRequiredService<TEndpoint>();
        endpoint.DefineEndpoints(app);
        return app;
    }
}
