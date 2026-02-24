using Ardalis.Specification;
using MicroCommerce.ApiService.Features.Ordering.Domain.Entities;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Specifications;

/// <summary>
/// Specification that excludes terminal order statuses (Failed, Cancelled) by default.
/// Optionally narrows to a specific active status via the statusFilter parameter.
/// </summary>
public sealed class ActiveOrdersSpec : Specification<Order>
{
    private static readonly OrderStatus[] TerminalStatuses = [OrderStatus.Failed, OrderStatus.Cancelled];

    public ActiveOrdersSpec(string? statusFilter = null)
    {
        Query.Where(o => !TerminalStatuses.Contains(o.Status));

        if (!string.IsNullOrWhiteSpace(statusFilter)
            && OrderStatus.TryFromName(statusFilter, ignoreCase: true, out OrderStatus? status))
        {
            Query.Where(o => o.Status == status);
        }
    }
}
