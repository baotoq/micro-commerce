using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Prometheus;

namespace MicroCommerce.Shared
{
    public static class StartupExtensions
    {
        public static void AddIdentityAuthentication(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var identityOptions = configuration.GetSection("Identity").Get<IdentityOptions>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = identityOptions.Uri.Internal;
                    options.Audience = identityOptions.Audience;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                });
        }

        public static void AddMonitoring(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var tracingOptions = configuration.GetSection("OpenTelemetry:Tracing").Get<TracingOptions>();

            services.AddHealthChecks().ForwardToPrometheus();

            services.AddOpenTelemetryTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .SetSampler(new AlwaysOnSampler())
                    .AddZipkinExporter(options =>
                    {
                        options.ServiceName = tracingOptions.ServiceName;
                        options.Endpoint = new Uri(tracingOptions.Endpoint);
                    });
            });
        }

        public static void UseMonitoring(this IApplicationBuilder app)
        {
            app.UseHttpMetrics();
            app.UseGrpcMetrics();
        }

        public static void MapHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions());
            endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
            endpoints.MapMetrics();
        }

        public static void UseSwaggerEndpoint(this IApplicationBuilder app, string name, string clientId = "swagger", string secret = "secret")
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId(clientId);
                c.OAuthClientSecret(secret);
                c.OAuthUsePkce();
                c.SwaggerEndpoint("/swagger/v1/swagger.json", name);
            });
        }
    }
}
