using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroCommerce.ApiService.Infrastructure;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder builder);
}

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        return services.AddEndpoints(Assembly.GetExecutingAssembly());
    }

    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where (type =>
                type is { IsAbstract: false, IsInterface: false } && type. IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor. Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null!)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
