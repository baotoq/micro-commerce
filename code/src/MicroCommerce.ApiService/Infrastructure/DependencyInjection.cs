using System.Reflection;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using MassTransit;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Exceptions;
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
    public static void AddInfrastructure(this IServiceCollection services, IConfigurationManager configuration)
    {
        // Add services to the container.
        services.AddProblemDetails();
        services.AddExceptionHandler<CustomExceptionHandler>();

        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddHttpContextAccessor();
        
        services.AddElasticsearch(configuration);
        services.AddMassTransit(configuration);
        services.AddEfCore(configuration);
        
        services.AddHealthChecks();
        services.AddRedLock(configuration);
        
        services.AddAuthorization();
        
        services.AddTransient<IDomainEventDispatcher, MassTransitDomainEventDispatcher>();
    }

    private static void AddRedLock(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var connectionString = configuration.GetConnectionString(AspireConstants.Redis) ?? "localhost:6379";
            return RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                ConnectionMultiplexer.Connect(connectionString)
            }, sp.GetRequiredService<ILoggerFactory>());
        });
    }

    private static void AddAuthorization(this IServiceCollection services)
    {
        services.AddIdentityApiEndpoints<User>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
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
    
    private static void AddEfCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, DateEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, IndexProductInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString(AspireConstants.Database);
            ArgumentNullException.ThrowIfNull(connectionString, "Database connection string is required.");
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
    }
    
    private static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var connectionString = configuration.GetConnectionString(AspireConstants.Elasticsearch);
            ArgumentNullException.ThrowIfNull(connectionString, "Elasticsearch connection string is required.");
            var settings= new ElasticsearchClientSettings(new Uri(connectionString))
                .Authentication(new BasicAuthentication("", ""))
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
    
    private static void AddMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(s =>
        {
            s.AddConsumers(Assembly.GetExecutingAssembly());
            s.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = configuration.GetConnectionString(AspireConstants.Messaging) ?? "amqp://guest:guest@localhost:5672";
                cfg.Host(new Uri(connectionString!), "/");
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