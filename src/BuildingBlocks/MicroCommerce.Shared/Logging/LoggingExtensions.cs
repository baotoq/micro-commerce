using Microsoft.Extensions.Hosting;
using Serilog;

namespace MicroCommerce.Shared.Logging
{
    public static class LoggingExtensions
    {
        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);
            });

            return hostBuilder;
        }
    }
}
