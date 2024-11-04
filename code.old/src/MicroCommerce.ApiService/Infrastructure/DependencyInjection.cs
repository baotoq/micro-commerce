using System.Reflection;
using Ardalis.GuardClauses;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using FluentValidation;
using MassTransit;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Exceptions;
using MicroCommerce.ApiService.Infrastructure.Behaviour;
using MicroCommerce.ApiService.Infrastructure.Interceptors;
using MicroCommerce.ServiceDefaults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace MicroCommerce.ApiService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddHttpContextAccessor();
        
        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        
        builder.AddElasticsearch();
        builder.AddMassTransit();
        builder.AddEfCore();
        builder.AddRedisDistributedCache(AspireConstants.Redis);
        builder.AddRedLock();
        builder.AddAuthorization();
    }
    
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddTransient<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();
        builder.Services.AddTransient<ICacheService, CacheService>();
    }

    private static void AddRedLock(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(AspireConstants.Redis);
        Guard.Against.NullOrEmpty(connectionString, message: "Redis connection string is required.");

        builder.Services.AddSingleton(sp => RedLockFactory.Create(new List<RedLockMultiplexer>
        {
            ConnectionMultiplexer.Connect(connectionString)
        }, sp.GetRequiredService<ILoggerFactory>()));
    }

    private static void AddAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddIdentityApiEndpoints<User>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
        builder.Services.AddAuthorizationBuilder();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });
    }
    
    private static void AddEfCore(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(AspireConstants.Database);
        Guard.Against.NullOrEmpty(connectionString, message: "Database connection string is required.");
        
        builder.Services.AddScoped<ISaveChangesInterceptor, DateEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, IndexProductInterceptor>();
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();
    }
    
    private static void AddElasticsearch(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(AspireConstants.Elasticsearch);
        Guard.Against.NullOrEmpty(connectionString, message: "Elasticsearch connection string is required.");
        
        builder.Services.AddSingleton(sp =>
        {
            var settings= new ElasticsearchClientSettings(new Uri(connectionString))
                .DefaultMappingFor<ProductDocument>(i => i
                    .IndexName(ElasticSearchIndexKey.Product.Key)
                    .IdProperty(p => p.Id)
                )
                .EnableDebugMode()
                .PrettyJson();

            var client = new ElasticsearchClient(settings);
            
            return client;
        });
    }
    
    private static void AddMassTransit(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(AspireConstants.Messaging);
        ArgumentNullException.ThrowIfNull(connectionString, "Messaging connection string is required.");
        
        builder.Services.AddMassTransit(s =>
        {
            s.AddConsumers(typeof(Program).Assembly);
            s.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(connectionString));
                cfg.ConfigureEndpoints(context);

                cfg.PrefetchCount = 1;
                cfg.AutoDelete = true;
                
                cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000, 2000, 5000));
            });
            
            // s.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            // {
            //     o.UsePostgres();
            //     o.UseBusOutbox();
            // });
        });
    }
}