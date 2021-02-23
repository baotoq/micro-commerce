using System;
using System.Reflection;
using AutoMapper;
using Grpc.Health.V1;
using MediatR;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Services;
using MicroCommerce.Shared;
using MicroCommerce.Shared.Grpc;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddGrpc();
            services.AddControllers().AddDapr();

            services.AddIdentityAuthentication();

            services.AddGrpcClient<Health.HealthClient>(options => options.Address = new Uri(Configuration["Client:Ordering:Uri:Grpc"]))
                .EnableCallContextPropagation(options => options.SuppressContextNotFoundErrors = true);

            services.AddSwagger();
            services.AddMonitoring();
            services.AddHealthChecks().AddNpgSql(connectionString).ForwardToPrometheus();

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, provider =>
                    provider.EnableRetryOnFailure()).UseSnakeCaseNamingConvention());

            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddTransient<IOrderingServiceClient, OrderingServiceClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseSwaggerEndpoint();
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
                endpoints.MapGrpcService<HealthService>();
            });
        }
    }
}
