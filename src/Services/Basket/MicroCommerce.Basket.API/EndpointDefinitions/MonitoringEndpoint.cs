using MicroCommerce.Shared.Monitoring;
using Prometheus;

namespace MicroCommerce.Basket.API.EndpointDefinitions;

public class MonitoringEndpoint : IEndpointDefinition
{
    public void DefineEnpoints(WebApplication app)
    {
        app.UseHttpMetrics();
        app.UseGrpcMetrics();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapSubscribeHandler();
        });
    }

    public void DefineServices(IServiceCollection services)
    {
        services.AddMonitoring();
    }
}
