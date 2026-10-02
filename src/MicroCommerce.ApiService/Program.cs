using FastEndpoints;
using FastEndpoints.OpenApi;
using MicroCommerce.ApiService.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<AppDbContext>("micro-commerce-db");
builder.AddRedisDistributedCache("redis");

builder.Services.AddProblemDetails();
builder.Services
    .AddFastEndpoints()
    .OpenApiDocument(o => o.DocumentName = "v1");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.UseFastEndpoints(c => c.Errors.UseProblemDetails());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.AddDocument("v1"));
}

app.MapDefaultEndpoints();

app.Run();
