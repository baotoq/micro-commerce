using System.Diagnostics;
using MassTransit;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.DomainEvents;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OpenTelemetry.Trace;

namespace MicroCommerce.MigrationService;

public class DbInitializer(IServiceProvider serviceProvider, ILogger<DbInitializer> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Initializing database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await InitializeDatabaseAsync(context, null, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }
    }

    public async Task InitializeDatabaseAsync(ApplicationDbContext context, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(context.Database.MigrateAsync, cancellationToken);

        await SeedDataAsync(context, cancellationToken);
        await IndexData(context, publishEndpoint, cancellationToken);

        logger.LogInformation("Database initialization completed after {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
    }

    private static async Task SeedDataAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (!context.Products.Any())
        {
            context.Products.Add(new Product { Name = "Product 1", Price = 10 });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task IndexData(ApplicationDbContext context, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken)
    {
        return;
        var tasks = new List<Task>();
        foreach (var product in context.Products)
        {
            tasks.Add(publishEndpoint.Publish(new IndexProductDomainEvent
            {
                ProductId = product.Id
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}
