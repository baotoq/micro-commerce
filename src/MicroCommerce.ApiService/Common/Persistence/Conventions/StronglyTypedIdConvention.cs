using System.Linq.Expressions;
using MicroCommerce.BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroCommerce.ApiService.Common.Persistence.Conventions;

public sealed class StronglyTypedIdConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (IConventionEntityType entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (IConventionProperty property in entityType.GetProperties())
            {
                Type clrType = property.ClrType;
                Type? baseType = clrType.BaseType;

                while (baseType is not null)
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
                    {
                        Type underlyingType = baseType.GetGenericArguments()[0];
                        ValueConverter converter = CreateValueConverter(clrType, underlyingType);
                        property.Builder.HasConversion(converter);
                        break;
                    }

                    baseType = baseType.BaseType;
                }
            }
        }
    }

    private static ValueConverter CreateValueConverter(Type stronglyTypedIdType, Type underlyingType)
    {
        ParameterExpression idParam = Expression.Parameter(stronglyTypedIdType, "id");
        MemberExpression valueAccess = Expression.Property(idParam, nameof(StronglyTypedId<Guid>.Value));
        LambdaExpression toProvider = Expression.Lambda(valueAccess, idParam);

        ParameterExpression valueParam = Expression.Parameter(underlyingType, "value");
        NewExpression construct = Expression.New(stronglyTypedIdType.GetConstructor([underlyingType])!, valueParam);
        LambdaExpression fromProvider = Expression.Lambda(construct, valueParam);

        Type converterType = typeof(ValueConverter<,>).MakeGenericType(stronglyTypedIdType, underlyingType);
        return (ValueConverter)Activator.CreateInstance(converterType, toProvider, fromProvider)!;
    }
}
