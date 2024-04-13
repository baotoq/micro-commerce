using System.Reflection;
using Api;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FluentValidation;
using Infrastructure.Behaviour;
using Infrastructure.Common.Options;
using Infrastructure.Interceptors;
using Infrastructure.Options;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddElasticsearch(configuration);
        services.AddEfCore();
        
        services.AddHealthChecks();
        services.AddRedLock();
        
        services.AddAuthorization();
        services.AddMassTransit();
        
        services.AddTransient<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();
    }

    private static void AddRedLock(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
            RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                ConnectionMultiplexer.Connect("localhost:6371")
            }, sp.GetRequiredService<ILoggerFactory>()));
    }

    private static void AddAuthorization(this IServiceCollection services)
    {
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
    }

    private static void AddMassTransit(this IServiceCollection services)
    {
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
    
    private static void AddEfCore(this IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, DateEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) => {
            options.UseNpgsql("name=ConnectionStrings:DefaultConnection");
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
    }
    
    private static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.Key));
        
        services.AddSingleton(s =>
        {
            var option = s.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            var settings= new ElasticsearchClientSettings(new Uri(option.Url))
                .DefaultMappingFor<ProductDocument>(i => i
                    .IndexName(ElasticSearchIndexKey.Product)
                    .IdProperty(p => p.Id)
                )
                .EnableDebugMode()
                .PrettyJson();
            return new ElasticsearchClient(settings);
        });
    }
}