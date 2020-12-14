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

        public virtual TimeSpan StartDelay { get; } = TimeSpan.Zero;
        public virtual TimeSpan DelayTime { get; } = TimeSpan.FromMinutes(1);
        public abstract Task ProcessAsync(CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{BackgroundName} is running", GetType().Name);

            await Task.Delay(StartDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (_logger.BeginScope(Guid.NewGuid()))
                {
                    try
                    {
                        _logger.LogInformation("{BackgroundName} is doing background work", GetType().Name);
                        await ProcessAsync(stoppingToken);
                        _logger.LogInformation("{BackgroundName} had done background work", GetType().Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{BackgroundName} error occurred executing", GetType().Name);
                    }
                }

                _logger.LogInformation("{BackgroundName} was waiting {Delay} before next execution", GetType().Name, DelayTime);
                await Task.Delay(DelayTime, stoppingToken);
            }
        }
    }
}
