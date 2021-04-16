using System;
using System.Reflection;
using AutoMapper;
using Grpc.Health.V1;
using MicroCommerce.Catalog.API.Infrastructure.Filters;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Services;
using MicroCommerce.Shared;
using MicroCommerce.Shared.EventBus;
using MicroCommerce.Shared.FileStorage;
using MicroCommerce.Shared.Grpc;
using MicroCommerce.Shared.Identity;
using MicroCommerce.Shared.MediatR;
using MicroCommerce.Shared.Monitoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddControllers(s => s.Filters.Add<CustomExceptionFilterAttribute>()).AddDapr();

            services.AddIdentityAuthentication();

            services.AddSwagger();
            services.AddMonitoring(builder => builder.AddDbContextCheck<ApplicationDbContext>());

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")).UseSnakeCaseNamingConvention());

            services.AddMediatR().AddValidators();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddTransient<IOrderingServiceClient, OrderingServiceClient>();

            services.AddFileStorage(Environment.WebRootPath);

            services.AddDaprEvenBus();
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

            app.UseRouting();
            app.UseCloudEvents();

            app.UseMonitoring();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks();
                endpoints.MapMetrics();
                endpoints.MapControllers();
                endpoints.MapSubscribeHandler();
            });
        }
    }
}
