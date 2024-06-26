using System.Reflection;
using FluentValidation;
using MassTransit;
using MediatR;
using MicroCommerce.ApiService;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Endpoints;
using MicroCommerce.ApiService.Exceptions;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Infrastructure.Behaviour;
using MicroCommerce.ApiService.UseCases.Database;
using MicroCommerce.ServiceDefaults;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
//
// builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
//     .ReadFrom.Configuration(hostingContext.Configuration));

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.AddInfrastructure();
builder.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    using var scope = app.Services.CreateScope();
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await mediator.Send(new MigrateDatabaseCommand());
        await mediator.Send(new SeedDataCommand());
    }
    catch (Exception e)
    {
        logger.LogError(e, "An error occurred while migrating the database or seeding data. {Message}", e.Message);
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseCors();
//app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapIdentityApi<User>().WithTags("identity");
app.MapCarts();
app.MapPayments();
app.MapCategories();
app.MapProducts();
app.MapSeed();

app.Run();
