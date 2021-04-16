using System;
using System.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

namespace MicroCommerce.Shared.Monitoring
{
    public static class MonitoringDependencyInjection
    {
        public static IServiceCollection AddMonitoring(this IServiceCollection services, Func<IHealthChecksBuilder, IHealthChecksBuilder> healthCheckBuilderFunc = null)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var tracingOptions = configuration.GetSection("OpenTelemetry:Tracing").Get<TracingOptions>();
            
            if (string.IsNullOrEmpty(tracingOptions.ServiceName))
            {
                tracingOptions.ServiceName = Assembly.GetCallingAssembly().GetName().Name;
            }

            if (string.IsNullOrEmpty(tracingOptions.Endpoint))
            {
                tracingOptions.Endpoint = "http://zipkin:9411/api/v2/spans";
            }

            if (healthCheckBuilderFunc is not null)
            {
                healthCheckBuilderFunc(services.AddHealthChecks()).ForwardToPrometheus();
            }
            else
            {
                services.AddHealthChecks().ForwardToPrometheus();
            }

            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .SetSampler(new AlwaysOnSampler())
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(tracingOptions.ServiceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddSqlClientInstrumentation(o => o.SetDbStatementForText = true)
                    .AddZipkinExporter(options =>
                    {
                        options.Endpoint = new Uri(tracingOptions.Endpoint);
                    });
            });

            return services;
        }

        public static void UseMonitoring(this IApplicationBuilder app)
        {
            app.UseHttpMetrics();
            app.UseGrpcMetrics();
        }

        public static void MapHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
    }
}
