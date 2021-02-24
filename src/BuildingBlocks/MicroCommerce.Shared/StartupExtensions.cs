using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HealthChecks.UI.Client;
using MicroCommerce.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Prometheus;

namespace MicroCommerce.Shared
{
    public static class StartupExtensions
    {
        public static void AddIdentityAuthentication(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
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

        public static IServiceCollection AddMonitoring(this IServiceCollection services, Func<IHealthChecksBuilder, IHealthChecksBuilder> healthCheckBuilderFunc = null)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var tracingOptions = configuration.GetSection("OpenTelemetry:Tracing").Get<TracingOptions>();
            if (string.IsNullOrEmpty(tracingOptions.ServiceName))
            {
                throw new ArgumentNullException(nameof(TracingOptions.ServiceName));
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
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddSqlClientInstrumentation(o => o.SetTextCommandContent = true)
                    .AddZipkinExporter(options =>
                    {
                        options.ServiceName = tracingOptions.ServiceName;
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

        public static void AddSwagger(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var identityOptions = configuration.GetSection("Identity").Get<IdentityOptions>();

            var title = Assembly.GetCallingAssembly().GetName().Name;

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(s => s.FullName);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{identityOptions.Uri.External}/connect/token", UriKind.RelativeOrAbsolute),
                            AuthorizationUrl = new Uri($"{identityOptions.Uri.External}/connect/authorize", UriKind.RelativeOrAbsolute),
                            Scopes = identityOptions.Scopes.ToDictionary(s => s)
                        }
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                });
            });
        }

        public static void UseSwaggerEndpoint(this IApplicationBuilder app, string clientId = "swagger", string secret = "secret")
        {
            var name = Assembly.GetCallingAssembly().GetName().Name + " v1";

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
