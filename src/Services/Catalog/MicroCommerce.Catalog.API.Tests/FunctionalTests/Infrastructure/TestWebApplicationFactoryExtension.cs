using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using MicroCommerce.Catalog.API.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Catalog.API.Tests.FunctionalTests.Infrastructure
{
    public static class TestWebApplicationFactoryExtension
    {
        public static HttpClient CreateAuthenticatedClient<TStartup>(this WebApplicationFactory<TStartup> factory) where TStartup : class
        {
            var client = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

                        services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

                        services.AddAuthorization(options =>
                        {
                            options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                            {
                                policy.AddAuthenticationSchemes("Test");
                                policy.RequireAuthenticatedUser();
                            });
                        });
                    });
                })
                .CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            return client;
        }

        public static HttpClient CreateInMemoryDbClient<TStartup>(this WebApplicationFactory<TStartup> factory, Action<ApplicationDbContext> seedAction = null)
            where TStartup : class
        {
            var client = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

                        using var scope = services.BuildServiceProvider().CreateScope();

                        var scopedServices = scope.ServiceProvider;
                        var context = scopedServices.GetRequiredService<ApplicationDbContext>();

                        seedAction?.Invoke(context);
                    });
                })
                .CreateClient();

            return client;
        }
    }
}
