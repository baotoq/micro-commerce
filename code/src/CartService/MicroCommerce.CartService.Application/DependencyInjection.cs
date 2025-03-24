using System.Diagnostics;
using FluentValidation;
using MicroCommerce.CartService.Application.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MicroCommerce.CartService.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id);
            };
        });
        builder.Services.AddExceptionHandler<InvalidValidationExceptionHandler>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        builder.Services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
    }
}
