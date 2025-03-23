using System.Diagnostics;
using FluentValidation;
using MicroCommerce.CartService.Application.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.CartService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                context.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id);
            };
        });
        services.AddExceptionHandler<InvalidValidationExceptionHandler>();
        services.AddHttpContextAccessor();

         services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

         return services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
    }
}
