using MicroCommerce.BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MicroCommerce.ApiService.Common.Persistence;

public static class ConcurrencyTokenConvention
{
    public static ModelBuilder ApplyConcurrencyTokenConvention(this ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IConcurrencyToken).IsAssignableFrom(entityType.ClrType))
            {
                IMutableProperty? versionProperty = entityType.FindProperty(nameof(IConcurrencyToken.Version));
                if (versionProperty is not null)
                {
                    versionProperty.IsConcurrencyToken = true;
                }
            }
        }

        return modelBuilder;
    }
}
