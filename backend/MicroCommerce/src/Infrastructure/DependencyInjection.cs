using System.Reflection;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Behaviour;
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
        services.AddHealthChecks();
        
        services.AddSingleton(sp =>
            RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                ConnectionMultiplexer.Connect("localhost:6371")
            }, sp.GetRequiredService<ILoggerFactory>()));
        
        services.AddScoped<ISaveChangesInterceptor, DateEntityInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) => {
            options.UseNpgsql("name=ConnectionStrings:DefaultConnection");
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
        
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
}