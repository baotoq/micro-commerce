using System.Reflection;
using Application.Common.AutoMapper;
using Application.Common.Options;
using Application.UseCases.Ping;
using Application.UseCases.Products.Events;
using Infrastructure.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Common;

public static class EndpointsExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/ping", async (IMediator mediator) => Results.Ok(await mediator.Send(new PingCommand())));
        endpoints.MapGet("/migrate", async (ApplicationDbContext context) =>
        {
            await context.Database.MigrateAsync();
            return Results.Ok("Migrated successfully");
        });
        
        endpoints.MapHealthChecks("/healthz");
    }
}

public static class WebApplicationBuilderExtensions
{
    public static void AddRequiredServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
        builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
        
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();
        
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options => {
            options.UseNpgsql(connectionString);
        });

        builder.Services.AddMassTransit(s =>
        {
            var messageBroker = builder.Configuration.GetSection(MessageBrokerOptions.Key).Get<MessageBrokerOptions>()!;
            
            s.SetKebabCaseEndpointNameFormatter();
            s.AddConsumers(Assembly.GetExecutingAssembly());
            s.UsingRabbitMq((context,cfg) =>
            {
                cfg.AutoDelete = true;
                cfg.Host(messageBroker.Host, messageBroker.Port, "/", h => {
                    h.Username(messageBroker.User);
                    h.Password(messageBroker.Password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}