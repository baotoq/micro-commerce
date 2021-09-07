using MicroCommerce.Basket.API.EndpointDefinitions;
using MicroCommerce.Basket.API.Models;
using MicroCommerce.Shared.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddEndpointDefinitions(typeof(BasketItem));
builder.Services.AddAuthorization();
builder.Services.AddIdentityAuthentication();

await using var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpointDefinitions();

app.Run();
