using System.Security.Cryptography;
using System.Text.Json;
using FastEndpoints;
using FastEndpoints.OpenApi;
using Mapster;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService.Features.Diagnostics;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.ApiService.Infrastructure.Mapping;
using MicroCommerce.ApiService.Infrastructure.Messaging;
using MicroCommerce.ApiService.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Temporalio.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<AppDbContext>("micro-commerce-db");
// One database, one schema per area (002 research R2).
builder.AddNpgsqlDbContext<AccountsDbContext>("micro-commerce-db",
    configureDbContextOptions: o => o.UseNpgsql(AccountsDbContext.ConfigureNpgsql));
builder.AddNpgsqlDbContext<CatalogDbContext>("micro-commerce-db",
    configureDbContextOptions: o => o.UseNpgsql(CatalogDbContext.ConfigureNpgsql));

// Product images in Azure Blob Storage, Azurite locally (constitution 1.1.0, research R7).
builder.AddAzureBlobServiceClient("blobs");
builder.Services.AddSingleton<MicroCommerce.ApiService.Features.Catalog.ProductImages.IProductImageStore,
    MicroCommerce.ApiService.Features.Catalog.ProductImages.BlobProductImageStore>();

// Development brings the schema up to date itself, retrying until PostgreSQL is reachable (FR-004).
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<DatabaseMigrationStatus>();
    builder.Services.AddHostedService<DatabaseMigrator>();
    builder.Services.AddHealthChecks().AddCheck<DatabaseMigrationHealthCheck>("database-migrations");
}

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainRuleViolationHandler>();
builder.Services.AddSingleton<TypeAdapterConfig>(MappingConfig.Create());
builder.Services.AddSingleton(TimeProvider.System);

// Keycloak access tokens. Authority, audience and RequireHttpsMetadata are bound from
// Authentication:Schemes:Bearer (the AppHost sets the Authority to the dev realm).
// JwtBearer stays the default scheme: the Dapr scheme is only for the sidecar's /dapr/* calls.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters.NameClaimType = "name";
        o.TokenValidationParameters.RoleClaimType = "roles";
    })
    // Checks the dapr-api-token header against APP_API_TOKEN, which the AppHost generates. Without one,
    // a random token nobody knows keeps /dapr/* closed instead of open (Principle IV).
    .AddDapr(o => o.Token = builder.Configuration["APP_API_TOKEN"] is { Length: > 0 } token
        ? token
        : Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.PlatformOperator, p => p.RequireRole(AuthPolicies.PlatformOperatorRole));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentAccount, HttpCurrentAccount>();
builder.Services.AddScoped<IMerchantDirectory, EfMerchantDirectory>();
builder.Services.AddScoped<CurrentMerchant>();
builder.Services.AddScoped<ICurrentMerchant>(sp => sp.GetRequiredService<CurrentMerchant>());
builder.Services.AddScoped<MicroCommerce.ApiService.Features.Catalog.MerchantProducts.MerchantCatalog>();
builder.Services.AddScoped<MicroCommerce.ApiService.Features.Catalog.Marketplace.IProductSearch, MicroCommerce.ApiService.Features.Catalog.Marketplace.PostgresProductSearch>();

// Messaging through the Dapr sidecar (research R6).
builder.Services.AddDaprClient();
builder.Services.AddSingleton<IIntegrationEventPublisher, DaprIntegrationEventPublisher>();
builder.Services.AddScoped<IInbox, Inbox>();

// Durable workflows: a hosted Temporal worker inside the API (research R5).
builder.Services.AddTemporalClient(options =>
{
    options.TargetHost = builder.Configuration["Temporal:Address"] ?? "localhost:7233";
    options.Namespace = builder.Configuration["Temporal:Namespace"] ?? "default";
});
builder.Services.AddHostedTemporalWorker(SampleProcessWorkflow.TaskQueue)
    .AddScopedActivities<SampleProcessActivities>()
    .AddWorkflow<SampleProcessWorkflow>();

builder.Services
    .AddFastEndpoints()
    .OpenApiDocument(o =>
    {
        o.DocumentName = "v1";
        o.ConfigureOpenApi = x => x.AddSchemaTransformer<VogenIdSchemaTransformer>();
    });

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
    c.Errors.UseProblemDetails();
    // RFC 9457 validation problems with one entry per field, e.g. errors.delaySeconds (FR-015).
    c.Errors.ProducesMetadataType = typeof(HttpValidationProblemDetails);
    c.Errors.ResponseBuilder = (failures, _, statusCode) => new HttpValidationProblemDetails(
        failures
            .GroupBy(f => string.Join('.', f.PropertyName.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName)))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray()))
    {
        Status = statusCode,
        Title = "One or more validation errors occurred.",
    };
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.AddDocument("v1"));
}

app.MapDefaultEndpoints();

await app.RunAsync();

public partial class Program;
