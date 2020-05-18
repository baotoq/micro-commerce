using Microsoft.Extensions.DependencyInjection;

namespace BShop.API.Data
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
