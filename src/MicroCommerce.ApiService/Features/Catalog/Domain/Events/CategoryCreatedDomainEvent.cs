using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using MicroCommerce.BuildingBlocks.Common.Events;

namespace MicroCommerce.ApiService.Features.Catalog.Domain.Events;

/// <summary>
/// Domain event raised when a new category is created.
/// Thin event containing only the category ID - consumers query for additional data if needed.
/// </summary>
public sealed record CategoryCreatedDomainEvent : DomainEvent
{
    public Guid CategoryId { get; }

    public CategoryCreatedDomainEvent(CategoryId categoryId)
    {
        CategoryId = categoryId.Value;
    }
}
