using System.Threading;
using System.Threading.Tasks;
using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Catalog.API.HealthCheck
{
    public class BasketHealthCheck : IHealthCheck
    {
        private readonly ILogger<BasketHealthCheck> _logger;
        private readonly Health.HealthClient _healthClient;

        public BasketHealthCheck(Health.HealthClient healthClient)
        {
            _logger = NullLogger<BasketHealthCheck>.Instance;
            _healthClient = healthClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var health = await _healthClient.CheckAsync(new HealthCheckRequest());

            _logger.LogInformation("Basket HealthCheck Status: {Status}", health.Status);

            return health.Status switch
            {
                HealthCheckResponse.Types.ServingStatus.Serving => HealthCheckResult.Healthy(),
                _ => HealthCheckResult.Unhealthy()
            };
        }
    }
}
