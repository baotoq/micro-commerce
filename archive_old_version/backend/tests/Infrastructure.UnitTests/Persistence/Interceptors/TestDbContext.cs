using Domain;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitTests.Persistence.Interceptors;

internal record TestEntityCreatedDomainEvent : DomainEventBase
{
}

internal record TestEntityUpdatedDomainEvent : DomainEventBase
{
}

internal class TestEntity : EntityBase, IDateEntity, ISoftDeleteEntity
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}

internal class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options): base(options)
    {
    }
    
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}
