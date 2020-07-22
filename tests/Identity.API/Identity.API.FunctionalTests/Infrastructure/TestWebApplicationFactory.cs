using System;
using System.Linq;
using Identity.API.Data;
using Identity.API.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.API.FunctionalTests.Infrastructure
{
    public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("InMemoryDbForTesting"));

                using var scope = services.BuildServiceProvider().CreateScope();

                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<ApplicationDbContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<User>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<Role>>();
                var logger = scopedServices.GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

                context.Database.EnsureCreated();

                try
                {
                    context.InitializeDbForTests();
                    userManager.CreateAsync(new User
                    {
                        Email = "admin@gmail.com",
                        UserName = "admin@gmail.com"
                    }, "1qazZAQ!");
                    roleManager.CreateAsync(new Role { Name = "Admin" });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the " +
                                        "database with messages. Error: {Message}", ex.Message);
                }
            });
        }
    }
}
