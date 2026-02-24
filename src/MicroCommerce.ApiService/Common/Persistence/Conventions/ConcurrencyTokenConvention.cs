using MicroCommerce.BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace MicroCommerce.ApiService.Common.Persistence.Conventions;

public sealed class ConcurrencyTokenConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (IConventionEntityType entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!typeof(IConcurrencyToken).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            IConventionProperty? versionProperty = entityType.FindProperty(nameof(IConcurrencyToken.Version));
            if (versionProperty is not null)
            {
                versionProperty.Builder.IsConcurrencyToken(true);
            }
        }
    }
}
