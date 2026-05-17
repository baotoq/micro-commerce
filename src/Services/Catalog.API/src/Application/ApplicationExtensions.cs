using MicroCommerce.Catalog.Application.Products.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Catalog.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationExtensions).Assembly));
        return services;
    }
}
