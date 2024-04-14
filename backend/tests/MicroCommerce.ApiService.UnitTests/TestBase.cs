using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MicroCommerce.ApiService.UnitTests;

public abstract class TestBase
{
    protected readonly ApplicationDbContext Context;

    protected TestBase()
    {
        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        Context = new ApplicationDbContext(contextOptions);

        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }
}
