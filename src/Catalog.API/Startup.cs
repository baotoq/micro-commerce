using System;
using Catalog.API.AppServices;
using Grpc.Health.V1;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddControllers();

            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var basketGrpcOptions = new Action<GrpcClientFactoryOptions>(options =>
            {
                options.Address = new Uri(Configuration["BasketUrl:Grpc"]);
            });

            services.AddGrpcClient<Basket.API.Basket.BasketClient>(basketGrpcOptions);
            services.AddGrpcClient<Health.HealthClient>(basketGrpcOptions);

            services.AddHealthChecks().AddCheck<BasketHealthCheck>();

            services.AddTransient<BasketClientService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
