using System.Reflection;
using Azure.Storage.Blobs;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Services;
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

if (app.Environment.IsDevelopment())
{
    // In development, create the blob container and queue if they don't exist.
    var fileService = app.Services.GetRequiredService<IFileService>();
    await fileService.CreateContainerIfNotExistsAsync();
}

app.UseExceptionHandler();
app.UseCors();

app.UseRequestTimeouts();

app.UseOutputCache();
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapOpenApiEndpoints();

app.MapIdentityApi<User>().WithTags("Identity");
app.MapEndpoints();

app.Run();
