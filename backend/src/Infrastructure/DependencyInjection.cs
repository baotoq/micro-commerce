using System.Reflection;
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
        services.AddRedLock(configuration);
        
        services.AddAuthorization();
        
        services.AddTransient<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();
    }

    private static void AddRedLock(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.Key));

        services.AddSingleton(sp =>
        {
            var option = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                ConnectionMultiplexer.Connect(option.ConnectionString)
            }, sp.GetRequiredService<ILoggerFactory>());
        });
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
    
    private static void AddEfCore(this IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, DateEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, IndexProductInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) => {
            options.UseNpgsql("name=ConnectionStrings:DefaultConnection");
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
    }
    
    private static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.Key));
        
        services.AddSingleton(sp =>
        {
            var option = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            var settings= new ElasticsearchClientSettings(new Uri(option.Url))
                .Authentication(new BasicAuthentication("", ""))
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