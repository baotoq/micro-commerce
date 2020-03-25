using System;
using Catalog.API.AppServices;
using Catalog.API.Grpc;
using Catalog.API.HealthCheck;
using Catalog.API.Infrastructure;
using Grpc.Health.V1;
using Grpc.Net.ClientFactory;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Catalog.API
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options => options.EnableDetailedErrors = _env.IsDevelopment());

            services
                .AddCustomControllers(Configuration)
                .AddCustomGrpcClient(Configuration)
                .AddCustomDbContext(Configuration)
                .AddCustomHealthChecks(Configuration);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration["IdentityUrl"];

                    options.RequireHttpsMetadata = false;

                    options.ApiName = "catalog-api";
                });

            services.AddTransient<BasketClientService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapGrpcService<HealthCheckService>();
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
                .AddTypeActivatedCheck<BasketHealthCheck>("basket", services.BuildServiceProvider().GetRequiredService<Health.HealthClient>());

            return services;
        }
    }
}
