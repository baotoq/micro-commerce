using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MicroCommerce.ApiService.Features.Ordering.Data;

/// <summary>
/// Used by <c>dotnet ef</c> at design time, when Aspire hasn't injected a connection string.
/// </summary>
public class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseNpgsql("Host=localhost;Database=micro-commerce-db", OrderingDbContext.ConfigureNpgsql)
            .Options;

        return new OrderingDbContext(options);
    }
}
