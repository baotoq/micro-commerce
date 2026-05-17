using MicroCommerce.Catalog.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;

namespace MicroCommerce.Catalog.Infrastructure;

public static class InfrastructureExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<AppDbContext>("microcommerce");
        return builder;
    }
}
