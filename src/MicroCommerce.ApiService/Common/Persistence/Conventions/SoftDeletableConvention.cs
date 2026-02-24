using System.Linq.Expressions;
using MicroCommerce.BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace MicroCommerce.ApiService.Common.Persistence.Conventions;

public sealed class SoftDeletableConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (IConventionEntityType entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            // Skip derived types to avoid duplicate filters in inheritance hierarchies
            if (entityType.BaseType is not null)
            {
                continue;
            }

            SetSoftDeleteFilterMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [entityType]);
        }
    }

    private static readonly System.Reflection.MethodInfo SetSoftDeleteFilterMethod =
        typeof(SoftDeletableConvention)
            .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

    private static void SetSoftDeleteFilter<TEntity>(IConventionEntityType entityType)
        where TEntity : class, ISoftDeletable
    {
        Expression<Func<TEntity, bool>> filter = e => !e.IsDeleted;
        entityType.SetQueryFilter(filter);
    }
}
