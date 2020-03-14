using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Catalog.API.Grpc
{
    public class HealthCheckService : HealthServiceImpl
    {
        private readonly Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService _healthCheckService;

        public HealthCheckService(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        public override async Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            var health = await _healthCheckService.CheckHealthAsync(context.CancellationToken);

            SetStatus(string.Empty,
                health.Status switch
                {
                    HealthStatus.Healthy => HealthCheckResponse.Types.ServingStatus.Serving,
                    _ => HealthCheckResponse.Types.ServingStatus.NotServing
                });

            return await base.Check(request, context);
        }
    }
}
