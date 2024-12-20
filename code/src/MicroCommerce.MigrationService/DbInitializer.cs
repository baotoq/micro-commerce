using System.Diagnostics;
using MassTransit;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.DomainEvents;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using Microsoft.AspNetCore.Identity;
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
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            await InitializeDatabaseAsync(context, publishEndpoint, fileService, environment, userManager, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }
    }

    public async Task InitializeDatabaseAsync(ApplicationDbContext context, IPublishEndpoint publishEndpoint, IFileService fileService, IHostEnvironment environment, UserManager<User> userManager,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(context.Database.MigrateAsync, cancellationToken);

        await SeedDataAsync(context, fileService, environment, userManager, cancellationToken);
        await IndexData(context, publishEndpoint, cancellationToken);

        logger.LogInformation("Database initialization completed after {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
    }

    private static async Task SeedDataAsync(ApplicationDbContext context, IFileService fileService, IHostEnvironment environment, UserManager<User> userManager, CancellationToken cancellationToken)
    {
        if (!context.Products.Any())
        {
            static List<Product> GetPreconfiguredItems()
        {
            return [
                new() { RemainingStock = 100, Name = ".NET Bot Black Hoodie", Price = 19.5M, ImageUrl = "1.png" },
                new() { RemainingStock = 100, Name = ".NET Black & White Mug", Price= 8.50M, ImageUrl = "2.png" },
                new() { RemainingStock = 100, Name = "Prism White T-Shirt", Price = 12, ImageUrl = "3.png" },
                new() { RemainingStock = 100, Name = ".NET Foundation T-shirt", Price = 12, ImageUrl = "4.png" },
                new() { RemainingStock = 100, Name = "Roslyn Red Sheet", Price = 8.5M, ImageUrl = "5.png" },
                new() { RemainingStock = 100, Name = ".NET Blue Hoodie", Price = 12, ImageUrl = "6.png" },
                new() { RemainingStock = 100, Name = "Roslyn Red T-Shirt", Price = 12, ImageUrl = "7.png" },
                new() { RemainingStock = 100, Name = "Kudu Purple Hoodie", Price = 8.5M, ImageUrl = "8.png" },
                new() { RemainingStock = 100, Name = "Cup<T> White Mug", Price = 12, ImageUrl = "9.png" },
                new() { RemainingStock = 100, Name = ".NET Foundation Sheet", Price = 12, ImageUrl = "10.png" },
                new() { RemainingStock = 100, Name = "Cup<T> Sheet", Price = 8.5M, ImageUrl = "11.png" },
                new() { RemainingStock = 100, Name = "Prism White TShirt", Price = 12, ImageUrl = "12.png" }
            ];
        }

            var products = GetPreconfiguredItems();

            var tasks = new List<Task>();
            foreach (var product in products)
            {
                var filePath = Path.Combine(environment.ContentRootPath, "Resources/Images", product.ImageUrl);
                if (!File.Exists(filePath))
                {
                    continue;
                }

                var uploadTask = Task.Run(async () =>
                {
                    await using var stream = File.OpenRead(filePath);
                    await fileService.UploadFileAsync(product.ImageUrl, stream, cancellationToken);
                }, cancellationToken);

                tasks.Add(uploadTask);
            }

            await Task.WhenAll(tasks);

            await context.Products.AddRangeAsync(products, cancellationToken);
        }

        if (!context.Users.Any())
        {
            var result = await userManager.CreateAsync(new User
            {
                UserName = "admin@micro-commerce.com",
                Email = "admin@micro-commerce.com",
                EmailConfirmed = true
            }, "P@ssw0rd");

            var error = result.Errors;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task IndexData(ApplicationDbContext context, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        foreach (var product in context.Products)
        {
            tasks.Add(publishEndpoint.Publish(new IndexProductDomainEvent
            {
                ProductId = product.Id
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);

        await context.SaveChangesAsync(cancellationToken);
    }
}
