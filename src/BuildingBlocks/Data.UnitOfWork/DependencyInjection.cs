using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Data.UnitOfWork
{
    public static class DependencyInjection
    {
        public static void AddUnitOfWork(this IServiceCollection services, Func<IDbConnection> connFactoryFunc)
        {
            services.AddScoped<IUnitOfWork>(resolver => new UnitOfWork(connFactoryFunc, resolver));
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient(typeof(IRepository<,>), typeof(Repository<,>));
        }
    }
}
