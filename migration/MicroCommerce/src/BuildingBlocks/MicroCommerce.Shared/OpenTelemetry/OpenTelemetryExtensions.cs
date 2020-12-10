using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace MicroCommerce.Shared.OpenTelemetry
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services)
        {
            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .SetSampler(new AlwaysOnSampler())
                    .AddZipkinExporter(option =>
                    {
                        option.ServiceName = Assembly.GetCallingAssembly().GetName().Name;
                        option.Endpoint = new Uri("http://localhost:9412/api/v2/spans");
                    });
            });

            return services;
        }
    }
}
