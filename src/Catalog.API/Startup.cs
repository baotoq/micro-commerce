using System;
using Catalog.API.AppServices;
using Catalog.API.Grpc;
using Catalog.API.Infrastructure;
using Grpc.Health.V1;
using Grpc.Net.ClientFactory;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Catalog.API
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
            services.AddGrpc();

            services
                .AddCustomControllers(Configuration)
                .AddCustomGrpcClient(Configuration)
                .AddCustomDbContext(Configuration)
                .AddCustomHealthChecks(Configuration);

            services.AddTransient<BasketClientService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGrpcService<HealthCheckService>();
                    endpoints.MapControllers();
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                });
        }
    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomControllers(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            return services;
        }

        public static IServiceCollection AddCustomGrpcClient(this IServiceCollection services, IConfiguration configuration)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var basketGrpcOptions = new Action<GrpcClientFactoryOptions>(options =>
            {
                options.Address = new Uri(configuration["BasketUrl:Grpc"]);
            });

            services.AddGrpcClient<Basket.API.Basket.BasketClient>(basketGrpcOptions);
            services.AddGrpcClient<Health.HealthClient>(basketGrpcOptions);

            return services;
        }

        public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CatalogContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("CatalogContext")));

            return services;
        }

        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddDbContextCheck<CatalogContext>()
                .AddCheck("basket", new BasketHealthCheck(services.BuildServiceProvider().GetRequiredService<Health.HealthClient>()));

            return services;
        }
    }
}
