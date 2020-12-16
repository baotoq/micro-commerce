using System;
using System.Collections.Generic;
using System.Linq;
using Grpc.Health.V1;
using MicroCommerce.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;

namespace MicroCommerce.Catalog.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwagger(Configuration);

            services.AddIdentityAuthentication();

            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            services.AddGrpcClient<Health.HealthClient>(options => options.Address = new Uri(Configuration["Client:Ordering:Uri:Grpc"]))
                .EnableCallContextPropagation(options => options.SuppressContextNotFoundErrors = true);

            services.AddMonitoring();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerEndpoint("MicroCommerce.Catalog.API v1");
            }

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseMonitoring();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks();
                endpoints.MapMetrics();
                endpoints.MapControllers();
            });
        }
    }

    public static class ConfigureServicesExtensions
    {
        public static void AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            var identityOptions = configuration.GetSection("Identity").Get<IdentityOptions>();

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(s => s.FullName);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MicroCommerce.Catalog.API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{identityOptions.Uri.External}/connect/token"),
                            AuthorizationUrl = new Uri($"{identityOptions.Uri.External}/connect/authorize"),
                            Scopes = identityOptions.Scopes.ToDictionary(s => s)
                        },
                    },
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        
                        new List<string> { "catalog-apssi" }
                    }
                });
            });
        }
    }
}
