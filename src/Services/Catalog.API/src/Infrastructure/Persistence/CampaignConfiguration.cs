using MicroCommerce.Catalog.Domain.Marketing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.Catalog.Infrastructure.Persistence;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => CampaignId.From(value));

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(c => c.Subject)
            .HasColumnName("subject")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.PreviewText)
            .HasColumnName("preview_text")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.AudienceKey)
            .HasColumnName("audience_key")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(c => c.TemplateKey)
            .HasColumnName("template_key")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(c => c.ScheduledAt)
            .HasColumnName("scheduled_at");

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(c => c.Status);

        builder.Property(c => c.FollowupEnabled)
            .HasColumnName("followup_enabled")
            .IsRequired();

        builder.Property(c => c.FeaturedProductSku)
            .HasColumnName("featured_product_sku")
            .HasMaxLength(20);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.OwnsOne(c => c.Followup, b =>
        {
            b.ToTable("campaign_followups");

            b.Property(f => f.Enabled)
                .HasColumnName("enabled")
                .IsRequired();

            b.Property(f => f.Kind)
                .HasColumnName("kind")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            b.Property(f => f.DelayDays)
                .HasColumnName("delay_days")
                .IsRequired();

            b.Property(f => f.Subject)
                .HasColumnName("subject")
                .HasMaxLength(200)
                .IsRequired();

            b.Property(f => f.Preview)
                .HasColumnName("preview")
                .HasMaxLength(200)
                .IsRequired();
        });
    }
}
