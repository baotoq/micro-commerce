using MicroCommerce.Catalog.Application.Customers.Dtos;
using MicroCommerce.Catalog.Domain.Customers;

namespace MicroCommerce.Catalog.Application.Customers;

internal static class CustomerMapping
{
    public static CustomerDto ToDto(
        Customer c,
        int lifetimeCount,
        decimal lifetimeSpend,
        DateTimeOffset? firstAt,
        DateTimeOffset? lastAt) =>
        new(
            c.Id.Value,
            c.Name,
            c.Email.Value,
            c.City,
            c.Tags,
            lifetimeCount,
            lifetimeSpend,
            firstAt,
            lastAt,
            c.CreatedAt);
}
