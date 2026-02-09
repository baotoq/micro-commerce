using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Infrastructure;

/// <summary>
/// Background service that periodically removes expired carts (30-day TTL).
/// Prevents abandoned carts from accumulating in the database.
/// </summary>
public sealed class CartExpirationService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CartExpirationService> _logger;

    public CartExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<CartExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<CartDbContext>();

                var deletedCount = await context.Carts
                    .Where(c => c.ExpiresAt <= DateTimeOffset.UtcNow)
                    .ExecuteDeleteAsync(stoppingToken);

                if (deletedCount > 0)
                {
                    _logger.LogInformation(
                        "Removed {Count} expired carts", deletedCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown - exit the loop
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired carts");
            }
        }
    }
}
