namespace MicroCommerce.Basket.API.EndpointDefinitions;

public class DaprEndpoint : IEndpointDefinition
{
    public void DefineEnpoints(WebApplication app)
    {
        app.UseCloudEvents();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapSubscribeHandler();
        });
    }

    public void DefineServices(IServiceCollection services)
    {
        services.AddDaprClient();
    }
}
