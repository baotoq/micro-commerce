using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MicroCommerce.Shared.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace MicroCommerce.Shared
{
    public static class SwaggerDependencyInjection
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var identityOptions = configuration.GetSection("Identity").Get<IdentityOptions>();

            var title = Assembly.GetCallingAssembly().GetName().Name;

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(s => s.FullName);
                c.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{identityOptions.Uri.External}/connect/token", UriKind.RelativeOrAbsolute),
                            AuthorizationUrl = new Uri($"{identityOptions.Uri.External}/connect/authorize", UriKind.RelativeOrAbsolute),
                            Scopes = identityOptions.Scopes.ToDictionary(s => s)
                        }
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                });
            });
        }

        public static void UseSwaggerEndpoint(this IApplicationBuilder app, string clientId = "swagger", string secret = "secret")
        {
            var name = Assembly.GetCallingAssembly().GetName().Name + " v1";

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId(clientId);
                c.OAuthClientSecret(secret);
                c.OAuthUsePkce();
                c.SwaggerEndpoint("/swagger/v1/swagger.json", name);
            });
        }
    }
}
