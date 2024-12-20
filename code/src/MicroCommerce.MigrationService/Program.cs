using MassTransit;
using MassTransit.Transports;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
using MicroCommerce.MigrationService;
using MicroCommerce.ServiceDefaults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(DbInitializer.ActivitySourceName));

builder.AddNpgsqlDbContext<ApplicationDbContext>("db", settings =>
    {
        settings.DisableRetry = true;
    },
    options =>
    {
        options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly)).UseSnakeCaseNamingConvention();
    });

builder.Services.AddMassTransit(s =>
{
    s.AddConsumers(typeof(Program).Assembly);
    s.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString("messaging");

        cfg.Host(host);
        cfg.ConfigureEndpoints(context);

        cfg.PrefetchCount = 1;
        cfg.AutoDelete = true;
    });

    s.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox(c => c.DisableDeliveryService());
        o.DisableInboxCleanupService();
    });
});

builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.AddAzureBlobClient("blobs");
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());
builder.Services.AddHealthChecks()
    .AddCheck<DbInitializerHealthCheck>("DbInitializer");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/reset", async (ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint,
        DbInitializer dbInitializer,
        IFileService fileService,
        IHostEnvironment environment, UserManager<User> userManager, CancellationToken cancellationToken) =>
    {
        // Delete and recreate the database. This is useful for development scenarios to reset the database to its initial state.
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbInitializer.InitializeDatabaseAsync(dbContext, publishEndpoint, fileService, environment, userManager, cancellationToken);

        return Results.Ok("ok");
    });
}

app.MapDefaultEndpoints();

app.MapOpenApiEndpoints();

app.Run();

// dotnet ef migrations add Init -s src/MicroCommerce.MigrationService -p src/MicroCommerce.MigrationService --context ApplicationDbContext
