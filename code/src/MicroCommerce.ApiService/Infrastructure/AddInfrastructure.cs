using System.Diagnostics;
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
using MicroCommerce.ApiService.Services;
using MicroCommerce.ApiService.Services.Elasticsearch;
using MicroCommerce.ServiceDefaults;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace MicroCommerce.ApiService.Infrastructure;

public static class AddInfrastructureDependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{Environment.MachineName} {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id);
            };
        });
        builder.Services.AddExceptionHandler<InvalidValidationExceptionHandler>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        builder.AddElasticsearch();
        builder.AddEfCore();
        builder.AddMassTransit();
        builder.AddRedisDistributedCache("redis");
        builder.AddRedLock();
        builder.AddAuthorization();
    }

    private static void AddRedLock(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDistributedLockFactory>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("redis");
            Guard.Against.NullOrEmpty(connectionString, message: "Redis connection string is required.");

            return RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                ConnectionMultiplexer.Connect(connectionString)
            }, sp.GetRequiredService<ILoggerFactory>());
        });
    }

    private static void AddAuthorization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddIdentityApiEndpoints<User>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddAuthorizationBuilder();

        builder.Services.AddCors(options => { options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()); });
    }

    private static void AddEfCore(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISaveChangesInterceptor, DateEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, IndexProductInterceptor>();
        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("db");
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetServices<DateEntityInterceptor>());
            options.AddInterceptors(sp.GetServices<DispatchDomainEventsInterceptor>());
            options.AddInterceptors(sp.GetServices<IndexProductInterceptor>());
        });
        builder.EnrichNpgsqlDbContext<ApplicationDbContext>();
    }

    private static void AddElasticsearch(this IHostApplicationBuilder builder)
    {
        builder.AddElasticsearchClient("elasticsearch", null, client =>
        {
            client.DefaultMappingFor<ProductDocument>(i => i
                .IndexName(ProductDocument.IndexPattern)
                .IdProperty(p => p.Id)
            )
            .EnableDebugMode();
        });
    }

    public static void AddMassTransit(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(s =>
        {
            s.AddConsumers(typeof(Program).Assembly);
            s.UsingRabbitMq((context, cfg) =>
            {
                var configuration = context.GetRequiredService<IConfiguration>();
                var host = configuration.GetConnectionString("messaging");

                cfg.Host(host);
                cfg.ConfigureEndpoints(context);

                cfg.PrefetchCount = 1;
                cfg.AutoDelete = true;
            });

            s.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
        });
    }
}
