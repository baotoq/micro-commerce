using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests;

internal static class DbContextFactory
{
    // Apply the real Infrastructure EF mappings so owned collections (Order lines/timeline/
    // refund, Customer tags, Campaign followup) form a valid model in-memory. Relational-only
    // config (table/column names, check constraints, precision) is ignored by the in-memory
    // provider. Pass a shared dbName to share one store across multiple contexts — required to
    // assert that a mutation actually PERSISTED (the only reliable way to catch a missing
    // .AsTracking() on a command handler, since EF defaults to NoTracking).
    internal static AppDbContext Create(string? dbName = null)
    {
        AppDbContext.ConfigureModel = mb =>
            mb.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
