using MicroCommerce.Catalog.Application;
using MicroCommerce.Catalog.Infrastructure;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Api.Endpoints;
using MicroCommerce.Catalog.Api.SeedData;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClientBuilder("cache")
    .WithOutputCache();
builder.AddInfrastructure();

builder.Services.AddApplication();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddDaprClient();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    if (app.Configuration.GetValue<bool>("SEED_PRODUCTS"))
    {
        await ProductSeeder.SeedAsync(db, app.Environment.ContentRootPath);
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseOutputCache();

app.MapDefaultEndpoints();
app.MapProductReadEndpoints();
app.MapProductWriteEndpoints();

app.UseFileServer();

app.Run();
