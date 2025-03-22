using MicroCommerce.BuildingBlocks.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseDefault();
app.MapDefaultEndpoints();

app.Run();
