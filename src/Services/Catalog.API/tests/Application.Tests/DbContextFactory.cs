using MicroCommerce.Catalog.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Tests;

internal static class DbContextFactory
{
    internal static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
