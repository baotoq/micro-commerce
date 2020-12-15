using System;
using System.Reflection;
using MicroCommerce.Identity.API.Configuration;
using MicroCommerce.Identity.API.Data;
using MicroCommerce.Identity.API.Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;

namespace MicroCommerce.Identity.API
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
            services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"),
                    provider => provider.EnableRetryOnFailure()).UseSnakeCaseNamingConvention());

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<User>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddRazorPages();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MicroCommerce.Identity.API", Version = "v1" });
            });

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
                        options.ServiceName = Assembly.GetExecutingAssembly().GetName().Name;
                        options.Endpoint = new Uri(Configuration["OpenTelemetry:ZipkinEndpoint"]);
                    });
            });

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.UserInteraction.ErrorUrl = "/Error";
            })
            .AddInMemoryIdentityResources(IdentityServerConfiguration.IdentityResources)
            .AddInMemoryApiResources(IdentityServerConfiguration.ApiResources)
            .AddInMemoryApiScopes(IdentityServerConfiguration.ApiScopes)
            .AddInMemoryClients(IdentityServerConfiguration.Clients(Configuration))
            .AddAspNetIdentity<User>()
            .AddDeveloperSigningCredential(); // not recommended for production - you need to store your key material somewhere secure
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroCommerce.Identity.API v1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

            app.UseStaticFiles();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseHttpMetrics();
            app.UseGrpcMetrics();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions());
                endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
                endpoints.MapMetrics();
                endpoints.MapRazorPages();
            });
        }
    }
}
