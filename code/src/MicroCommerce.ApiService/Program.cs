using MediatR;
using MicroCommerce.ApiService.Exceptions;
using MicroCommerce.ApiService.Features;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.AddInfrastructure();
builder.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var api = app.MapGroup("api");

var products = api.MapGroup("products");

products.MapGet("/{id:guid}", async (Guid id, IMediator mediator) => await mediator.Send(new GetProduct.Query { Id = id }));
products.MapPost("/", async (CreateProduct.Command request, IMediator mediator) => await mediator.Send(request));

app.Run();
