using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UnitOfWork
{
    public static class ConfigureExtensions
    {
        public static void AddUnitOfWork<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction = null) where TDbContext : DbContext
        {
            services.AddDbContext<TDbContext>(optionsAction);

            services.AddScoped<DbContext, TDbContext>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
