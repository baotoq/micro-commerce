using MicroCommerce.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;

namespace MicroCommerce.Infrastructure;

public static class InfrastructureExtensions
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<AppDbContext>("microcommerce");
        return builder;
    }
}
