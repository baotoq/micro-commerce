using MicroCommerce.Shared.Monitoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;

namespace MicroCommerce.Shared.Common;

public class MonitoringEndpoint : IEndpointDefinition
{
    public void DefineEndpoints(WebApplication app)
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
