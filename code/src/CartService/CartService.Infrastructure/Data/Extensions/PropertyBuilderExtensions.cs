using MicroCommerce.CartService.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroCommerce.CartService.Infrastructure.Data.Extensions;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<Money> HasMoneyConversion(this PropertyBuilder<Money> builder)
    {
        return builder.HasConversion(s => s.Amount, value => new Money(value));
    }
}
