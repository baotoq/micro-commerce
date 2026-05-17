using MicroCommerce.Application;
using MicroCommerce.Infrastructure;
using MicroCommerce.Infrastructure.Persistence;
using MicroCommerce.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClientBuilder("cache")
    .WithOutputCache();
builder.AddInfrastructure();

builder.Services.AddApplication();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
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
