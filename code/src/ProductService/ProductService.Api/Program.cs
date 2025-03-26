using MicroCommerce.BuildingBlocks.ServiceDefaults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseDefault();
app.MapDefaultEndpoints();

app.MapGet("/api/products", ([FromServices]ILogger<Program> logger) =>
{
    logger.LogInformation("Get products");
    return new[]
    {
        new { Id = 1, Name = "products 1" }, new { Id = 2, Name = "products 2" },
        new { Id = 3, Name = "products 3" },
    };
});

app.Run();
