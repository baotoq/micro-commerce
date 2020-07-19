using System.Net.Http;
using System.Net.Http.Headers;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.API.FunctionalTests.Infrastructure
{
    public static class TestWebApplicationFactoryExtension
    {
        public static HttpClient CreateAuthenticatedClient<TStartup>(this TestWebApplicationFactory<TStartup> factory) where TStartup : class
        {
            var client = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.AddAuthentication("Test")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                                "Test", options => { });

                        services.AddAuthorization(options =>
                        {
                            options.AddPolicy(IdentityServerConstants.LocalApi.AuthenticationScheme, policy =>
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
    }
}
