using System;
using System.Collections.Generic;
using IdentityServer4;
using MicroCommerce.Identity.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace MicroCommerce.Identity.API.Configuration
{
    public static class StartupExtensions
    {
        public static void AddDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString,
                    provider => provider.EnableRetryOnFailure()).UseSnakeCaseNamingConvention());
        }

        //public static void AddSwagger(this IServiceCollection services)
        //{
        //    services.AddSwaggerGen(c =>
        //    {
        //        c.SwaggerDoc("v1", new OpenApiInfo { Title = "MicroCommerce.Identity.API", Version = "v1" });
        //        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        //        {
        //            Type = SecuritySchemeType.OAuth2,
        //            Flows = new OpenApiOAuthFlows
        //            {
        //                AuthorizationCode = new OpenApiOAuthFlow
        //                {
        //                    TokenUrl = new Uri($"/connect/token", UriKind.Relative),
        //                    AuthorizationUrl = new Uri($"/connect/authorize", UriKind.Relative),
        //                    Scopes =
        //                    {
        //                        {IdentityServerConstants.LocalApi.ScopeName, ""}
        //                    }
        //                },
        //            },
        //        });
        //        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        //        {
        //            {
        //                new OpenApiSecurityScheme
        //                {
        //                    Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
        //                },
        //                new List<string> {IdentityServerConstants.LocalApi.ScopeName}
        //            }
        //        });
        //    });
        //}
    }
}
