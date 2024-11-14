using MicroCommerce.ApiService.Exceptions;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.AddInfrastructure();
builder.AddApplication();

builder.AddElasticsearchClient("elasticsearch");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapGet("/", (context) =>
{
    return context.Response.WriteAsync("Hello World!");
});

app.MapGet("/a", (context) =>
{
    throw new NullReferenceException("hello");
    return context.Response.WriteAsync("Hello World!");
});

app.Run();
