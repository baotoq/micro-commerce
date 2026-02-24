using MicroCommerce.BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace MicroCommerce.ApiService.Common.Persistence.Conventions;

public sealed class AuditableConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (IConventionEntityType entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            IConventionProperty? createdAt = entityType.FindProperty(nameof(IAuditable.CreatedAt));
            if (createdAt is not null)
            {
                createdAt.Builder.HasColumnType("timestamp with time zone");
                createdAt.Builder.IsRequired(true);
            }

            IConventionProperty? updatedAt = entityType.FindProperty(nameof(IAuditable.UpdatedAt));
            if (updatedAt is not null)
            {
                updatedAt.Builder.HasColumnType("timestamp with time zone");
                updatedAt.Builder.IsRequired(true);
            }
        }
    }
}
