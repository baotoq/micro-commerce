using MicroCommerce.Domain.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.ApiService.Features.Diagnostics.Data;

internal sealed class SampleProcessConfiguration : IEntityTypeConfiguration<SampleProcess>
{
    public void Configure(EntityTypeBuilder<SampleProcess> builder)
    {
        builder.ToTable("sample_processes");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.Delay).HasColumnName("delay");
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>();
        builder.Property(p => p.StartedAt).HasColumnName("started_at");
        builder.Property(p => p.CompletedAt).HasColumnName("completed_at");
        builder.Ignore(p => p.DueAt);
        builder.Ignore(p => p.DomainEvents);
    }
}
