using System.Reflection;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.AddInfrastructure();
builder.AddApplication();
builder.Services.AddEndpoints();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app
    .MapDefaultEndpoints()
    .UseOpenApi()
    .MapEndpoints();

app.Run();
