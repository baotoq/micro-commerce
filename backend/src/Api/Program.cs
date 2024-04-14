using System.Reflection;
using Api;
using Api.Endpoints;
using Api.Exceptions;
using Api.UseCases.Products.DomainEvents;
using Domain.Entities;
using FluentValidation;
using Infrastructure;
using Infrastructure.Behaviour;
using Infrastructure.Common.Options;
using Infrastructure.Interceptors;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration));

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

app.Use(async (context, next) =>
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    await next.Invoke();
});

app.UseExceptionHandler(_ => {});
app.UseCors();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<User>().WithTags("identity");
app.MapGet("/", () => Results.Redirect("/healthz"));
app.MapHealthChecks("/healthz");
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
            cfg.Host(option.Host, option.Port, "/", h => {
                h.Username(option.User);
                h.Password(option.Password);
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