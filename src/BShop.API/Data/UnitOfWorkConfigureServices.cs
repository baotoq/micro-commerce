using Microsoft.Extensions.DependencyInjection;

namespace BShop.API.Data
{
    public static class UnitOfWorkConfigureServices
    {
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
