using System;
using System.Collections.Generic;
using BShop.API.Data;
using IdentityServer4.AccessTokenValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace BShop.API
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
            services.AddCors();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "BShop API", Version = "v1"});
                c.AddSecurityDefinition(IdentityServerAuthenticationDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{Configuration["Identity:Uri"]}/connect/token"),
                            AuthorizationUrl = new Uri($"{Configuration["Identity:Uri"]}/connect/authorize"),
                            Scopes =
                            {
                                { "bshop-api", "BShop API" }
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
                        new List<string> { "bshop-api" }
                    }
                });
            });

            services.AddControllers();
            services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

            services.AddMediatR(typeof(Startup));

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration["Identity:Uri"];
                    options.ApiName = "bshop-api";
                    options.ApiSecret = "secret";

                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(10); // that's the default
                });

            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId("swagger");
                c.OAuthClientSecret("secret");
                c.OAuthUsePkce();
                c.RoutePrefix =  string.Empty;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BShop API V1");
            });

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
}
