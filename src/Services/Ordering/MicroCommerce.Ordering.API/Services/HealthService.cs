using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.Ordering.API.Services
{
    public class HealthService : Health.HealthBase
    {
        private readonly ILogger<HealthService> _logger;
        private readonly HealthCheckService _healthCheckService;

        public HealthService(HealthCheckService healthCheckService, ILogger<HealthService> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        public override async Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            HealthReport healthReport;

            if (string.IsNullOrWhiteSpace(request.Service))
            {
                healthReport = await _healthCheckService.CheckHealthAsync();
            }
            else
            {
                healthReport = await _healthCheckService.CheckHealthAsync(
                    s => string.Equals(s.Name, request.Service, StringComparison.OrdinalIgnoreCase));

                if (healthReport.Entries.Count == 0)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, $"Service {request.Service} not found"));
                }
            }

            _logger.LogInformation("HealthReport {@HealthReport}", healthReport);

            return new HealthCheckResponse
            {
                Status = healthReport.Status switch
                {
                    HealthStatus.Degraded => HealthCheckResponse.Types.ServingStatus.Serving,
                    HealthStatus.Healthy => HealthCheckResponse.Types.ServingStatus.Serving,
                    HealthStatus.Unhealthy => HealthCheckResponse.Types.ServingStatus.NotServing,
                    _ => HealthCheckResponse.Types.ServingStatus.Unknown,
                }
            };
        }
    }
}
