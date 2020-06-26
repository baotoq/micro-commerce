using System;
using System.Collections.Generic;
using Catalog.API.BackgroundServices;
using Catalog.API.Data;
using Catalog.API.Services;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared.MediatR;
using UnitOfWork;

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
            services.AddMediatR().AddValidators();

            services.AddUnitOfWork<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    provider => provider.EnableRetryOnFailure()));

            services.AddSwagger(Configuration);

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration["Identity:Uri"];
                    options.ApiName = "catalog-api";
                    options.ApiSecret = "secret";
                    options.RequireHttpsMetadata = false;
                });

            services.AddHttpContextAccessor();

            services.AddCors();
            services.AddControllers().AddNewtonsoftJson();
            services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

            services.AddHostedService<ApproveReviewBackgroundService>();
            services.AddHostedService<ApproveReplyBackgroundService>();

            services.AddScoped<IIdentityService, IdentityService>();
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

            app.UseSwaggerDefault();

            app.UseRouting();

            app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog API", Version = "v1" });
                c.AddSecurityDefinition(IdentityServerAuthenticationDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{configuration["Identity:Uri"]}/connect/token"),
                            AuthorizationUrl = new Uri($"{configuration["Identity:Uri"]}/connect/authorize"),
                            Scopes =
                            {
                                { "catalog-api", "Catalog API" }
                            }
                        },
                    },
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = IdentityServerAuthenticationDefaults.AuthenticationScheme }
                        },
                        new List<string> { "catalog-api" }
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
                c.SwaggerEndpoint("swagger/v1/swagger.json", "Catalog API V1");
            });
        }
    }
}
