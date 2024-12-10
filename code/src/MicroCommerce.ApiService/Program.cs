using System.Reflection;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options => options.AddDefaultPolicy(
    policy => policy
        .WithOrigins("*://localhost:*")
        .AllowAnyMethod()
        .AllowAnyHeader()
    )
);


// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.AddInfrastructure();
builder.AddApplication();
builder.Services.AddEndpoints();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

app.UseRequestTimeouts();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapDefaultEndpoints();
app.MapOpenApiEndpoints();

app.MapIdentityApi<User>().WithTags("Identity");
app.MapEndpoints();

app.Run();
