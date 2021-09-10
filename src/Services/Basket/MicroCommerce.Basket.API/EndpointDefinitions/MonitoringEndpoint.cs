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
            endpoints.MapHealthChecks();
            endpoints.MapMetrics();
        });
    }

    public void DefineServices(IServiceCollection services)
    {
        services.AddMonitoring();
    }
}
