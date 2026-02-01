using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Inventory.Infrastructure;

/// <summary>
/// Background service that periodically removes expired stock reservations.
/// Prevents expired reservations from accumulating and skewing available quantity calculations.
/// </summary>
public sealed class ReservationCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationCleanupService> _logger;

    public ReservationCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationCleanupService> logger)
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
                var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

                var expired = await context.StockReservations
                    .Where(r => r.ExpiresAt <= DateTimeOffset.UtcNow)
                    .ToListAsync(stoppingToken);

                if (expired.Count > 0)
                {
                    context.StockReservations.RemoveRange(expired);
                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Removed {Count} expired stock reservations", expired.Count);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown - exit the loop
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired stock reservations");
            }
        }
    }
}
