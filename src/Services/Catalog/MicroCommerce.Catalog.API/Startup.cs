using System;
using System.Reflection;
using AutoMapper;
using Dapr.Client;
using Grpc.Health.V1;
using MicroCommerce.Catalog.API.Infrastructure.Filters;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Services;
using MicroCommerce.Shared;
using MicroCommerce.Shared.EventBus;
using MicroCommerce.Shared.EventBus.Abstractions;
using MicroCommerce.Shared.FileStorage;
using MicroCommerce.Shared.Grpc;
using MicroCommerce.Shared.MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Serilog;

namespace MicroCommerce.Catalog.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddGrpc();
            services.AddControllers(s => s.Filters.Add<CustomExceptionFilterAttribute>()).AddDapr();

            services.AddIdentityAuthentication();

            services.AddGrpcClient<Health.HealthClient>(options => options.Address = new Uri(Configuration["Client:Ordering:Uri:Grpc"]))
                .EnableCallContextPropagation(options => options.SuppressContextNotFoundErrors = true);

            services.AddSwagger();
            services.AddMonitoring(builder => builder.AddNpgSql(connectionString));

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

            services.AddMediatR().AddValidators();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddTransient<IOrderingServiceClient, OrderingServiceClient>();

            services.AddFileStorage(Environment.WebRootPath);

            services.AddScoped<IEventBus>(resolver =>
                new DaprEventBus("pubsub", resolver.GetRequiredService<DaprClient>(), resolver.GetRequiredService<ILogger<DaprEventBus>>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseSwaggerEndpoint();
            }

            app.UseSerilogRequestLogging();

            app.UseCloudEvents();

            app.UseRouting();

            app.UseMonitoring();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks();
                endpoints.MapMetrics();
                endpoints.MapControllers();
                endpoints.MapGrpcService<HealthService>();
                endpoints.MapSubscribeHandler();
            });
        }
    }
}
