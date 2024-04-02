using Domain;
using FluentAssertions;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Infrastructure.UnitTests.Persistence.Interceptors;

public class DateEntityInterceptorTests
{
    private readonly TestDbContext _context;
    
    public DateEntityInterceptorTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new DateEntityInterceptor())
            .Options;
        
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task When_Added_ShouldAutoPopulateCreatedAt()
    {
        var entity = new TestEntity();
        
        await _context.TestEntities.AddAsync(entity);

        await _context.SaveChangesAsync();

        entity.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.Date);
        entity.UpdatedAt.Should().BeNull();
    }
    
    [Fact]
    public async Task When_Updated_ShouldAutoPopulateUpdatedAt()
    {
        var entity = new TestEntity
        {
            Id = "TestId"
        };
        
        await _context.TestEntities.AddAsync(entity);

        await _context.SaveChangesAsync();
        
        entity = await _context.TestEntities.FindAsync("TestId");
        
        entity!.Name = "Test";
        entity.AddDomainEvent(new TestEntityUpdatedDomainEvent());

        await _context.SaveChangesAsync();

        entity.UpdatedAt.Should().BeAfter(DateTimeOffset.UtcNow.Date);
    }
}