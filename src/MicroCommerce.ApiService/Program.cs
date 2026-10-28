using FastEndpoints;
using FastEndpoints.OpenApi;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.Features.Catalog.Data;
using MicroCommerce.ApiService;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<AccountsDbContext>("micro-commerce-db",
    configureDbContextOptions: o => o.UseNpgsql(AccountsDbContext.ConfigureNpgsql));
builder.AddNpgsqlDbContext<CatalogDbContext>("micro-commerce-db",
    configureDbContextOptions: o => o.UseNpgsql(CatalogDbContext.ConfigureNpgsql));
builder.AddRedisDistributedCache("redis");
builder.AddMessaging();

// Keycloak access tokens. Authority, audience and RequireHttpsMetadata are bound from
// Authentication:Schemes:Bearer (the AppHost sets the Authority to the dev realm).
builder.Services.AddAuthentication().AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters.NameClaimType = "name";
    o.TokenValidationParameters.RoleClaimType = "roles";
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.PlatformOperator, p => p.RequireRole(AuthPolicies.PlatformOperatorRole));
builder.Services.AddScoped<CurrentAccount>();
builder.Services.AddScoped<CurrentMerchant>();
builder.Services.AddScoped<MerchantCatalog>();
builder.Services.AddScoped<MerchantStatusChanges>();
builder.Services.AddScoped<IProductSearch, PostgresProductSearch>();
builder.Services.AddSingleton<StorefrontProductCache>();

builder.Services.AddProblemDetails();
builder.Services
    .AddFastEndpoints()
    .OpenApiDocument(o =>
    {
        o.DocumentName = "v1";
        // Value objects (typed IDs, ShopName, Money) appear in the schema as the primitives they wrap.
        o.ConfigureOpenApi = openApi => openApi.MapVogenTypesInMicroCommerce_ApiService();
    });

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<AccountsDbContext>().Database.MigrateAsync();
    var catalog = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await catalog.Database.MigrateAsync();
    await StarterCategories.SeedAsync(catalog);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c => c.Errors.UseProblemDetails());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.AddDocument("v1"));
}

app.MapDefaultEndpoints();

app.Run();
