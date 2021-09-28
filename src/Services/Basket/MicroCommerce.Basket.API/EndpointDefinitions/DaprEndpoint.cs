using MicroCommerce.Shared.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Basket.API.EndpointDefinitions;

public class DaprEndpoint : IEndpointDefinition
{
    public void DefineEndpoints(WebApplication app)
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
