using System.Reflection;
using FluentValidation;
using MassTransit;
using MediatR;
using MicroCommerce.ApiService;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Endpoints;
using MicroCommerce.ApiService.Exceptions;
using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.ApiService.Infrastructure.Behaviour;
using MicroCommerce.ApiService.Infrastructure.Common.Options;
using MicroCommerce.ServiceDefaults;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration));

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
AddMassTransit(builder.Services, builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseCors();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapIdentityApi<User>().WithTags("identity");
app.MapGet("/", () => Results.Redirect("/alive"));
app.MapCarts();
app.MapCategories();
app.MapProducts();
app.MapSeed();

app.Run();

void AddMassTransit(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<MessageBrokerOptions>(configuration.GetSection(MessageBrokerOptions.Key));
        
    services.AddMassTransit(s =>
    {
        s.AddConsumers(Assembly.GetExecutingAssembly());
        s.UsingRabbitMq((context, cfg) =>
        {
            var option = context.GetRequiredService<IOptions<MessageBrokerOptions>>().Value;
            var connectionString = configuration.GetConnectionString("rabbitmq");
            cfg.Host(new Uri(connectionString!), "/", h => {
            });
            cfg.ConfigureEndpoints(context);

            cfg.PrefetchCount = 1;
            cfg.AutoDelete = true;
                
            cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000, 1000, 1000, 1000, 1000));
        });
            
        // s.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
        // {
        //     o.UsePostgres();
        //     o.UseBusOutbox();
        // });
    });
}