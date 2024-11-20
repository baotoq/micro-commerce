using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.MigrationService;
using MicroCommerce.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource("MicroCommerce.MigrationService"));

builder.AddNpgsqlDbContext<ApplicationDbContext>("db", settings =>
    {
        settings.DisableRetry = true;
    },
    options =>
    {
        options.UseNpgsql(b => b.MigrationsAssembly(typeof(Program).Assembly));
    });

builder.Services.AddHostedService<DbInitializer>();
builder.Services.AddHealthChecks()
    .AddCheck<DbInitializerHealthCheck>("DbInitializer");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/reset", async (ApplicationDbContext dbContext, DbInitializer dbInitializer, CancellationToken cancellationToken) =>
    {
        // Delete and recreate the database. This is useful for development scenarios to reset the database to its initial state.
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbInitializer.InitializeDatabaseAsync(dbContext, cancellationToken);
    });
}

app.MapDefaultEndpoints();

app.UseOpenApi();

app.Run();
