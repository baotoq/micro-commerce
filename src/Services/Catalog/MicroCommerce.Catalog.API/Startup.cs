using System;
using Grpc.Health.V1;
using MicroCommerce.Shared;
using MicroCommerce.Shared.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddGrpc();
            services.AddControllers();

            services.AddIdentityAuthentication();

            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            services.AddGrpcClient<Health.HealthClient>(options => options.Address = new Uri(Configuration["Client:Ordering:Uri:Grpc"]))
                .EnableCallContextPropagation(options => options.SuppressContextNotFoundErrors = true);

            services.AddSwagger();
            services.AddMonitoring();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
