using System.Reflection;
using Api;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FluentValidation;
using Infrastructure.Behaviour;
using Infrastructure.Common.Options;
using Infrastructure.Interceptors;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        var settings= new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
            .DefaultMappingFor<ProductDocument>(i => i
                .IndexName(ElasticSearchIndexKey.Product)
                .IdProperty(p => p.Id)
            )
            .EnableDebugMode()
            .PrettyJson();
        services.AddSingleton(new ElasticsearchClient(settings));
        
        services.AddHealthChecks();
        
        services.AddSingleton(sp =>
            RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                ConnectionMultiplexer.Connect("localhost:6371")
            }, sp.GetRequiredService<ILoggerFactory>()));
        
        services.AddScoped<ISaveChangesInterceptor, DateEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) => {
            options.UseNpgsql("name=ConnectionStrings:DefaultConnection");
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
        
        services.AddTransient<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();

        services.AddIdentityCore<User>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();
        
        services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
        services.AddAuthorizationBuilder();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
        });
        
        services.AddMassTransit(s =>
        {
            var messageBroker = new MessageBrokerOptions();
            
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
            
            // s.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            // {
            //     o.UsePostgres();
            //     o.UseBusOutbox();
            // });
        });
    }
}