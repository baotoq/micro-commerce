using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Catalog.API.BackgroundServices
{
    public abstract class BaseBackgroundService : BackgroundService
    {
        private readonly ILogger<BaseBackgroundService> _logger;

        protected IServiceProvider ServiceProvider { get; }

        protected BaseBackgroundService(IServiceProvider services)
        {
            ServiceProvider = services;
            _logger = services.GetRequiredService<ILogger<BaseBackgroundService>>();
        }

        public abstract string BackgroundName { get; }
        public abstract TimeSpan DelayTime { get; }
        public abstract Task ProcessAsync(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{BackgroundName} is running", BackgroundName);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (_logger.BeginScope(Guid.NewGuid()))
                {
                    try
                    {
                        _logger.LogInformation("{BackgroundName} is doing background work", BackgroundName);
                        await ProcessAsync(stoppingToken);
                        _logger.LogInformation("{BackgroundName} have done background work", BackgroundName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{BackgroundName} error occurred executing", BackgroundName);
                    }
                }

                _logger.LogInformation("{BackgroundName} is waiting {delay} before next execute", BackgroundName, DelayTime);
                await Task.Delay(DelayTime, stoppingToken);
            }
        }
    }
}
