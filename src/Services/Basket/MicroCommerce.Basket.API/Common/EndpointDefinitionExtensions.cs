namespace MicroCommerce.Basket.API.EndpointDefinitions;

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
}
