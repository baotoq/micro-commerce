using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MicroCommerce.Catalog.API.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Catalog.API.Tests.IntegrationTests.Infrastructure
{
    public static class TestWebApplicationFactoryExtension
    {
        public static HttpClient CreateAuthenticatedClient<TStartup>(this WebApplicationFactory<TStartup> factory, Func<ApplicationDbContext, Task> seedAction = null) where TStartup : class
        {
            var client = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddAuthentication("Test")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

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
                .CreateUnauthenticatedClient(seedAction, new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            return client;
        }

        public static HttpClient CreateUnauthenticatedClient<TStartup>(
            this WebApplicationFactory<TStartup> factory,
            Func<ApplicationDbContext, Task> seedAction = null,
            WebApplicationFactoryClientOptions options = null)
            where TStartup : class
        {
            var client = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        var databaseName = Guid.NewGuid().ToString();
                        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(databaseName));

                        if (seedAction is not null)
                        {
                            using var scope = services.BuildServiceProvider().CreateScope();

                            var scopedServices = scope.ServiceProvider;
                            var context = scopedServices.GetRequiredService<ApplicationDbContext>();

                            seedAction.Invoke(context).GetAwaiter().GetResult();
                        }
                    });
                })
                .CreateClient(options ?? new WebApplicationFactoryClientOptions());

            return client;
        }

        public static TClient CreateGrpcClient<TClient>(this WebApplicationFactory<Startup> factory)
            where TClient : ClientBase<TClient>
        {
            var channel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                HttpClient = factory.CreateUnauthenticatedClient()
            });

            return Activator.CreateInstance(typeof(TClient), channel) as TClient;
        }
    }
}
