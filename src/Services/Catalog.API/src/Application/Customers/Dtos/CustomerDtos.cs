namespace MicroCommerce.Catalog.Application.Customers.Dtos;

public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    string City,
    IReadOnlyList<string> Tags,
    int LifetimeOrderCount,
    decimal LifetimeSpend,
    DateTimeOffset? FirstOrderAt,
    DateTimeOffset? LastOrderAt,
    DateTimeOffset CreatedAt);
