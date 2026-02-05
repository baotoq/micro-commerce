using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Common.Persistence;

public class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Outbox tables in 'outbox' schema
        modelBuilder.HasDefaultSchema("outbox");

        // MassTransit outbox entities for transactional messaging
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
