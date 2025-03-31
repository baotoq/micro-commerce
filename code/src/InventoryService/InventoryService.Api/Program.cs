using MicroCommerce.InventoryService.Application;
using MicroCommerce.InventoryService.Infrastructure;
using MicroCommerce.BuildingBlocks.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplication();
builder.AddInfrastructure();

builder.Services.AddCors(options => options.AddDefaultPolicy(
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseExceptionHandler();
app.UseDefault();

app.UseRequestTimeouts();

app.MapDefaultEndpoints();

await app.RunAsync();
