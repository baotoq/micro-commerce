using MicroCommerce.BuildingBlocks.ServiceDefaults;
using MicroCommerce.CartService.Application;
using MicroCommerce.CartService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseDefault();
app.MapDefaultEndpoints();

app.MapGet("/api/carts", () => new[]
{
    new { Id = 1, Name = "carts 1" },
    new { Id = 2, Name = "carts 2" },
    new { Id = 3, Name = "carts 3" },
});

await app.RunAsync();
