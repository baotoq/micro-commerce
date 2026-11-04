using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Infrastructure.Messaging;

/// <summary>An inbox row: this consumer has handled this message (FR-012b).</summary>
public sealed class ProcessedEvent
{
    public required string Consumer { get; init; }

    public required string MessageId { get; init; }

    public DateTimeOffset ProcessedAt { get; init; }
}

internal sealed class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");
        builder.HasKey(e => new { e.Consumer, e.MessageId });
        builder.Property(e => e.Consumer).HasColumnName("consumer");
        builder.Property(e => e.MessageId).HasColumnName("message_id");
        builder.Property(e => e.ProcessedAt).HasColumnName("processed_at");
    }
}
