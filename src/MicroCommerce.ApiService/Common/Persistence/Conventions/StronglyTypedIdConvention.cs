using System.Linq.Expressions;
using System.Reflection;
using MicroCommerce.BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroCommerce.ApiService.Common.Persistence.Conventions;

public sealed class StronglyTypedIdConvention : IModelFinalizingConvention
{
    private static readonly MethodInfo CreateConverterMethod =
        typeof(StronglyTypedIdConvention).GetMethod(nameof(CreateTypedConverter), BindingFlags.NonPublic | BindingFlags.Static)!;

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
                        ValueConverter converter = CreateConverter(clrType, underlyingType);
                        property.Builder.HasConversion(converter);
                        break;
                    }

                    baseType = baseType.BaseType;
                }
            }
        }
    }

    private static ValueConverter CreateConverter(Type stronglyTypedIdType, Type underlyingType)
    {
        MethodInfo genericMethod = CreateConverterMethod.MakeGenericMethod(stronglyTypedIdType, underlyingType);
        return (ValueConverter)genericMethod.Invoke(null, null)!;
    }

    private static ValueConverter<TId, TUnderlying> CreateTypedConverter<TId, TUnderlying>()
        where TId : StronglyTypedId<TUnderlying>
    {
        ParameterExpression idParam = Expression.Parameter(typeof(TId), "id");
        MemberExpression valueAccess = Expression.Property(idParam, nameof(StronglyTypedId<TUnderlying>.Value));
        Expression<Func<TId, TUnderlying>> toProvider = Expression.Lambda<Func<TId, TUnderlying>>(valueAccess, idParam);

        ConstructorInfo? constructor = typeof(TId).GetConstructor([typeof(TUnderlying)]);
        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"StronglyTypedId '{typeof(TId).Name}' must have a constructor accepting a single '{typeof(TUnderlying).Name}' parameter.");
        }

        ParameterExpression valueParam = Expression.Parameter(typeof(TUnderlying), "value");
        NewExpression construct = Expression.New(constructor, valueParam);
        Expression<Func<TUnderlying, TId>> fromProvider = Expression.Lambda<Func<TUnderlying, TId>>(construct, valueParam);

        return new ValueConverter<TId, TUnderlying>(toProvider, fromProvider);
    }
}
