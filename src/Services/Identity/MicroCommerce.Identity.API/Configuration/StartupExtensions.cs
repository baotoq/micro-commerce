using MicroCommerce.Identity.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Identity.API.Configuration
{
    public static class StartupExtensions
    {
        public static void AddDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString,
                    provider => provider.EnableRetryOnFailure()).UseSnakeCaseNamingConvention());
        }
    }
}
