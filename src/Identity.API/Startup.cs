using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Identity.API
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids => new IdentityResource[]
        {
            new IdentityResources.OpenId()
        };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("catalog-api", "Catalog API")
        };

        public static IEnumerable<Client> Clients => new[]
        {
            new Client
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = { "catalog-api" }
            }
        };

        public static List<TestUser> TestUsers => new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "1",
                Username = "alice",
                Password = "password"
            },
            new TestUser
            {
                SubjectId = "2",
                Username = "bob",
                Password = "password"
            }
        };
    }

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
            services.AddHealthChecks();

            services.AddControllers();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddTestUsers(Config.TestUsers)
                .AddDeveloperSigningCredential();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
