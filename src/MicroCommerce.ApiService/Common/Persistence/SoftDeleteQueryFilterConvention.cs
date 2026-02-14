using System.Linq.Expressions;
using MicroCommerce.BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MicroCommerce.ApiService.Common.Persistence;

public static class SoftDeleteQueryFilterConvention
{
    public static ModelBuilder ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
                MemberExpression property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                UnaryExpression notDeleted = Expression.Not(property);
                LambdaExpression filter = Expression.Lambda(notDeleted, parameter);

                entityType.SetQueryFilter(filter);
            }
        }

        return modelBuilder;
    }
}
