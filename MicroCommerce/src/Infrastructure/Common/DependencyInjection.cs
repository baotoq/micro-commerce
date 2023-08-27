using System.Reflection;
using Infrastructure.Common.Options;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Common;

public static class DependencyInjection
{
    public static void AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));
        
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
                cfg.Host(messageBroker.Host, messageBroker.Port, "/", h => {
                    h.Username(messageBroker.User);
                    h.Password(messageBroker.Password);
                });
                cfg.ConfigureEndpoints(context);

                cfg.PrefetchCount = 1;
                cfg.AutoDelete = true;
            });
        });
    }
}