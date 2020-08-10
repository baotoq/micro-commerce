using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Bshop.Shared.V1;
using Catalog.API.BackgroundServices;
using Catalog.API.Consumers;
using Catalog.API.Data;
using Catalog.API.Grpc;
using Catalog.API.Services;
using GreenPipes;
using Grpc.HealthCheck;
using IdentityServer4.AccessTokenValidation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Shared.FileStorage;
using Shared.Grpc;
using Shared.MediatR;
using UnitOfWork;
using static Bshop.Identity.V1.IdentityService;

namespace Catalog.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = Environment.IsDevelopment();
            });

            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            services.AddGrpcClient<IdentityServiceClient>(options =>
            {
                options.Address = new Uri(Configuration["Identity:Uri:Grpc"]);
            }).EnableCallContextPropagation(options => options.SuppressContextNotFoundErrors = true);
            services.AddGrpcClient<PingService.PingServiceClient>(options =>
            {
                options.Address = new Uri(Configuration["Identity:Uri:Grpc"]);
            }).EnableCallContextPropagation(options => options.SuppressContextNotFoundErrors = true);

            services.AddMediatR().AddValidators();

            services.AddUnitOfWork<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"),
                    provider => provider.EnableRetryOnFailure()));

            services.AddResponseCaching();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379";
            });

            services.AddSwagger(Configuration);

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration["Identity:Uri:Http"];
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
            services.AddFileStorage(Environment.WebRootPath);

            services.AddMassTransitHostedService();
            services.AddMassTransit(s =>
            {
                s.AddConsumersFromNamespaceContaining<BaseConsumer<BaseMessage>>();

                s.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitmq = Configuration.GetSection("Rabbitmq");
                    cfg.Host(new Uri(rabbitmq["Uri"]), hostConfig =>
                    {
                        hostConfig.Username(rabbitmq["UserName"]);
                        hostConfig.Password(rabbitmq["Password"]);
                    });
                    cfg.ReceiveEndpoint("catalog-api", s =>
                    {
                        s.PrefetchCount = rabbitmq.GetValue<ushort>("PrefetchCount");
                        s.ConfigureConsumer<TestConsumer>(context);
                    });
                    cfg.UseMessageRetry(r =>
                    {
                        r.Immediate(rabbitmq.GetValue<int>("RetryLimit"));
                    });
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseResponseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions
                {
                    ResponseWriter = WriteResponse
                });
                endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self"),
                });
                endpoints.MapControllers();
                endpoints.MapGrpcService<HealthServiceImpl>();
                endpoints.MapGrpcService<PingGrpcService>();
                endpoints.MapGrpcService<CatalogGrpcService>();
            });
        }

        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(
                            p => new JProperty(p.Key, JsonConvert.SerializeObject(p.Value)))))))))));

            return context.Response.WriteAsync(json.ToString(Formatting.Indented));
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
                            TokenUrl = new Uri($"{configuration["Identity:Uri:Http"]}/connect/token"),
                            AuthorizationUrl = new Uri($"{configuration["Identity:Uri:Http"]}/connect/authorize"),
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
                c.SwaggerEndpoint("swagger/v1/swagger.json", "Identity API V1");
            });
        }
    }
}
