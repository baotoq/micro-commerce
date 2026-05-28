using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MicroCommerce.Catalog.Application;
using MicroCommerce.Catalog.Infrastructure;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Api.Endpoints;
using MicroCommerce.Catalog.Api.ExceptionHandlers;
using MicroCommerce.Catalog.Api.SeedData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClientBuilder("cache")
    .WithOutputCache();
builder.AddInfrastructure();
builder.AddAzureBlobServiceClient("photos");

builder.Services.AddApplication();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddDaprClient();
builder.Services.AddExceptionHandler<InvalidInputExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    if (app.Configuration.GetValue<bool>("SEED_PRODUCTS"))
    {
        await ProductSeeder.SeedAsync(db, app.Environment.ContentRootPath);
    }

    // SAS uploads PUT directly from the browser to the blob endpoint. In dev
    // that endpoint is Azurite, which has no CORS rules out of the box, so
    // browsers block the preflight. Configure permissive CORS for the
    // Aspire-served origins on startup; in production this should be set by
    // the storage account's own CORS policy.
    if (app.Environment.IsDevelopment())
    {
        var blobClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
        var props = await blobClient.GetPropertiesAsync();
        props.Value.Cors.Clear();
        props.Value.Cors.Add(new BlobCorsRule
        {
            AllowedOrigins = "*",
            AllowedMethods = "PUT,GET,OPTIONS,HEAD",
            AllowedHeaders = "*",
            ExposedHeaders = "*",
            MaxAgeInSeconds = 3600,
        });
        await blobClient.SetPropertiesAsync(props.Value);
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
