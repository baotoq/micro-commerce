using FluentAssertions;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.UnitTests.Persistence.Interceptors;

public class SoftDeleteInterceptorTests
{
    private readonly TestDbContext _context;
    
    public SoftDeleteInterceptorTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new SoftDeleteInterceptor())
            .Options;
        
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task ShouldNotPermanentDeleteEntity()
    {
        var entity = new TestEntity()
        {
            Id = "TestId"
        };
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();

        _context.Remove(entity);
        await _context.SaveChangesAsync();

        entity = await _context.TestEntities.FindAsync("TestId");
        entity.Should().NotBeNull();
        entity!.DeletedAt.Should().NotBeNull();
    }
}