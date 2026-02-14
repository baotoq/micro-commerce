using System.Security.Claims;
using FluentValidation;
using MassTransit;
using MicroCommerce.ApiService.Common.Behaviors;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Common.Messaging;
using MicroCommerce.ApiService.Common.Messaging.Exceptions;
using MicroCommerce.ApiService.Common.Persistence;
using MicroCommerce.ApiService.Features.Cart;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Catalog.Infrastructure;
using MicroCommerce.ApiService.Features.Messaging;
using MicroCommerce.ApiService.Features.Inventory;
using MicroCommerce.ApiService.Features.Inventory.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using MicroCommerce.ApiService.Features.Ordering.Infrastructure;
using MicroCommerce.ApiService.Features.Profiles;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using MicroCommerce.ApiService.Features.Reviews;
using MicroCommerce.ApiService.Features.Reviews.Infrastructure;
using MicroCommerce.ApiService.Features.Wishlists;
using MicroCommerce.ApiService.Features.Wishlists.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Outbox DbContext for transactional domain events
builder.AddNpgsqlDbContext<OutboxDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "outbox"));
});

// Module DbContexts
builder.AddNpgsqlDbContext<CatalogDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "catalog"));
});

builder.AddNpgsqlDbContext<CartDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "cart"));
});

builder.AddNpgsqlDbContext<OrderingDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ordering"));
});

builder.AddNpgsqlDbContext<InventoryDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory"));
});

builder.AddNpgsqlDbContext<ProfilesDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "profiles"));
});

builder.AddNpgsqlDbContext<ReviewsDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "reviews"));
});

builder.AddNpgsqlDbContext<WishlistsDbContext>("appdb", configureDbContextOptions: options =>
{
    options.UseNpgsql(npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "wishlists"));
});

// Azure Blob Storage for product images
builder.AddAzureBlobServiceClient("blobs");

// Azure Service Bus client for DLQ management (Aspire integration registers ServiceBusClient)
builder.AddAzureServiceBusClient("messaging");

// MassTransit with Azure Service Bus and EF Core outbox
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);

    x.AddSagaStateMachine<CheckoutStateMachine, CheckoutState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.ExistingDbContext<OrderingDbContext>();
            r.UsePostgres();
        });

    x.AddEntityFrameworkOutbox<OutboxDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
    });

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        // DLQ routing for Azure Service Bus endpoints
        if (cfg is IServiceBusReceiveEndpointConfigurator sb)
        {
            sb.ConfigureDeadLetterQueueErrorTransport();
        }

        // Circuit breaker (outermost) - stops consuming after sustained failures
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        // Retry with exponential backoff - PermanentException skips retries
        cfg.UseMessageRetry(r =>
        {
            r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(25));
            r.Ignore<PermanentException>();
        });

        // Inbox deduplication (innermost) - prevents duplicate message processing
        cfg.UseEntityFrameworkOutbox<OutboxDbContext>(context);
    });

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<DomainEventInterceptor>();

// Add services to the container.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: "micro-commerce",
        options =>
        {
            // Accept tokens issued to the nextjs-app client
            // The 'azp' (authorized party) claim will be 'nextjs-app'
            options.TokenValidationParameters.ValidateAudience = false;

            // For development only - disable HTTPS metadata validation
            // In production, use explicit Authority configuration instead
            if (builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });
builder.Services.AddAuthorizationBuilder();

// MediatR with validation pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();

    // Validation runs first - fail fast before handler executes
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// FluentValidation - auto-discover validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Catalog services
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddHostedService<CatalogDataSeeder>();

// Inventory services
builder.Services.AddHostedService<ReservationCleanupService>();
builder.Services.AddHostedService<InventoryDataSeeder>();

// Cart services
builder.Services.AddHostedService<CartExpirationService>();

// Messaging services
builder.Services.AddScoped<IDeadLetterQueueService, DeadLetterQueueService>();

// Profile services
builder.Services.AddScoped<IAvatarImageService, AvatarImageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries =
    ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

// Protected endpoint - returns user info from JWT token
app.MapGet("/me", (ClaimsPrincipal user) =>
    {
        var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return new
        {
            IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
            Name = user.Identity?.Name,
            Email = user.FindFirst("email")?.Value,
            Subject = user.FindFirst("sub")?.Value,
            Claims = claims
        };
    })
    .WithName("GetCurrentUser")
    .RequireAuthorization();

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .RequireAuthorization();

app.MapDefaultEndpoints();

// Module endpoints
app.MapCatalogEndpoints();
app.MapInventoryEndpoints();
app.MapCartEndpoints();
app.MapOrderingEndpoints();
app.MapMessagingEndpoints();
app.MapProfilesEndpoints();
app.MapReviewsEndpoints();
app.MapWishlistsEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make Program class accessible to WebApplicationFactory in tests
public partial class Program { }
