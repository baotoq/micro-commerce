using MicroCommerce.Basket.API.Models;
using MicroCommerce.Shared.Common;
using MicroCommerce.Shared.Identity;
using MicroCommerce.Shared.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureLogging();

builder.Services.AddEndpointDefinitions(typeof(BasketItem));
builder.Services.AddEndpointDefinition<MonitoringEndpoint>();
builder.Services.AddAuthorization();
builder.Services.AddIdentityAuthentication();

await using var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpointDefinitions();
app.UseEndpointDefinition<MonitoringEndpoint>();

await app.RunAsync();
