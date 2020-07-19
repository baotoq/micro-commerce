using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Identity.API.Data;
using Identity.API.Data.Models;
using Identity.API.Services;
using IdentityServer4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared.MediatR;
using UnitOfWork;
using Grpc.HealthCheck;
using Identity.API.Grpc;

namespace Identity.API
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
            void DbContextOptions(DbContextOptionsBuilder builder) =>
                builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    provider =>
                    {
                        provider.EnableRetryOnFailure();
                        provider.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                    });

            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddMediatR().AddValidators();

            services.AddUnitOfWork<ApplicationDbContext>(DbContextOptions);

            services
                .AddDefaultIdentity<User>()
                .AddRoles<Role>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.UserInteraction.ErrorUrl = "/Error";
                })
                .AddConfigurationStore(options => options.ConfigureDbContext = DbContextOptions)
                .AddOperationalStore(options => options.ConfigureDbContext = DbContextOptions)
                .AddAspNetIdentity<User>()
                .AddProfileService<ProfileService>()
                .AddDeveloperSigningCredential(); // not recommended for production - you need to store your key material somewhere secure

            services.AddLocalApiAuthentication();

            services.AddCors();
            services.AddRazorPages();
            services.AddControllers().AddNewtonsoftJson();
            services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

            services.AddSwagger(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UsePathBase(Configuration["PathBase"]);

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSwaggerDefault();

            app.UseRouting();

            app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());

            app.UseIdentityServer();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/health");
                endpoints.MapGrpcService<HealthServiceImpl>();
                endpoints.MapGrpcService<PingGrpcService>();
                endpoints.MapGrpcService<IdentityGrpcService>();
            });
        }
    }

    public static class StartupConfigureServices
    {
        public static void AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(s => s.FullName);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity API", Version = "v1" });
                c.AddSecurityDefinition(IdentityServerConstants.LocalApi.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri("/connect/token", UriKind.Relative),
                            AuthorizationUrl = new Uri("/connect/authorize", UriKind.Relative),
                            Scopes =
                            {
                                { IdentityServerConstants.LocalApi.ScopeName, IdentityServerConstants.LocalApi.ScopeName }
                            }
                        },
                    },
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = IdentityServerConstants.LocalApi.AuthenticationScheme }
                        },
                        new List<string> { IdentityServerConstants.LocalApi.ScopeName }
                    }
                });
            });
        }

        public static void UseSwaggerDefault(this IApplicationBuilder app)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;
                c.OAuthClientId("swagger");
                c.OAuthClientSecret("secret");
                c.OAuthUsePkce();
                c.SwaggerEndpoint("swagger/v1/swagger.json", "Identity API V1");
            });
        }
    }
}
