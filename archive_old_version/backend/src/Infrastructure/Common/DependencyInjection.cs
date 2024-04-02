using System.Reflection;
using Infrastructure.Common.Options;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        
        builder.Services.AddScoped<ISaveChangesInterceptor, DateEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, SoftDeleteInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) => {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        builder.Services.AddTransient<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();
        
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
                
                cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000, 1000, 1000, 1000, 1000));
            });
            
            s.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
        });
    }
}