using MassTransit;
using MassTransit.Transports;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.MigrationService;
using MicroCommerce.ServiceDefaults;
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
        options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly));
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

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());
builder.Services.AddHealthChecks()
    .AddCheck<DbInitializerHealthCheck>("DbInitializer");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/reset", async (ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, DbInitializer dbInitializer, CancellationToken cancellationToken) =>
    {
        // Delete and recreate the database. This is useful for development scenarios to reset the database to its initial state.
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbInitializer.InitializeDatabaseAsync(dbContext, publishEndpoint, cancellationToken);
    });
}

app.MapDefaultEndpoints();

app.UseOpenApi();

app.Run();

// dotnet ef migrations add Init -s src/MicroCommerce.MigrationService -p src/MicroCommerce.MigrationService --context ApplicationDbContext
