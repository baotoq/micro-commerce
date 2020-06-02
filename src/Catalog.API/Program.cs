using System;
using System.Threading.Tasks;
using Catalog.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Catalog.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().MigrateDatabase<ApplicationDbContext>().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public static class Extensions
    {
        public static IHost MigrateDatabase<T>(this IHost webHost) where T : DbContext
        {
            Task.Run(async () =>
            {
                using var scope = webHost.Services.CreateScope();
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<T>();

                try
                {
                    if (context.Database.IsSqlServer())
                    {
                        logger.LogInformation("Start migrating database");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Migrating database was successful");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database");
                }

                try
                {
                    logger.LogInformation("Start seeding database");
                    await context.InitializeDataAsync();
                    logger.LogInformation("Seeding database was successful");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database");
                }
            });
            return webHost;
        }
    }
}
