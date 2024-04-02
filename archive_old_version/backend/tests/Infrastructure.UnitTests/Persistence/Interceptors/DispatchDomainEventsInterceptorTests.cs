using Domain;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Infrastructure.UnitTests.Persistence.Interceptors;

public class DispatchDomainEventsInterceptorTests
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly TestDbContext _context;
    
    public DispatchDomainEventsInterceptorTests()
    {
        _domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();
        
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new DispatchDomainEventsInterceptor(_domainEventDispatcher))
            .Options;
        
        _context = new TestDbContext(options);
    }

    [Fact]
    public async Task When_Added_ShouldDispatchDomainEvent()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestEntityCreatedDomainEvent());
        
        await _context.TestEntities.AddAsync(entity);

        await _context.SaveChangesAsync();

        await _domainEventDispatcher.Received(1).DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>());
    }
    
    [Fact]
    public async Task When_Added_ShouldDispatchThreeDomainEvent()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestEntityCreatedDomainEvent());
        entity.AddDomainEvent(new TestEntityCreatedDomainEvent());
        entity.AddDomainEvent(new TestEntityCreatedDomainEvent());
        
        await _context.TestEntities.AddAsync(entity);

        await _context.SaveChangesAsync();

        await _domainEventDispatcher.Received().DispatchAsync(Arg.Is<IEnumerable<IDomainEvent>>(s => s.Count() == 3));
    }
    
    [Fact]
    public async Task When_Added_ShouldNotDispatchDomainEvent()
    {
        var entity = new TestEntity();
        await _context.TestEntities.AddAsync(entity);

        await _context.SaveChangesAsync();

        await _domainEventDispatcher.DidNotReceive().DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>());
    }
    
    [Fact]
    public async Task When_Updated_ShouldDispatchDomainEvent()
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
        
        await _domainEventDispatcher.Received().DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>());
    }
}