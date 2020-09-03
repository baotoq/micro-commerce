using System;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Data.UnitOfWork
{
    public static class DependencyInjection
    {
        public static void AddUnitOfWork(this IServiceCollection services, Func<DbConnection> connFactoryFunc, SimpleCRUD.Dialect dialect)
        {
            SimpleCRUD.SetDialect(dialect);
            services.AddScoped<Func<DbConnection>>(resolver => () => new TracingDbConnection(connFactoryFunc(), resolver.GetService<ILoggerFactory>()));
            services.AddTransient<IConnectionFactory, ConnectionFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient(typeof(IRepository<,>), typeof(Repository<,>));
        }
    }
}
