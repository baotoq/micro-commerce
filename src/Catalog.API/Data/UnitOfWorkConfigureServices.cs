using Microsoft.Extensions.DependencyInjection;

namespace Catalog.API.Data
{
    public static class UnitOfWorkConfigureServices
    {
        public static void AddUnitOfWork(this IServiceCollection services)
        {
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
