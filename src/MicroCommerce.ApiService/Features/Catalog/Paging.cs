namespace MicroCommerce.ApiService.Features.Catalog;

/// <summary>One page of a list, with the total across every page.</summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

/// <summary>Paging rules shared by every list: default page 1 and size 20; a size above 100 is clamped.</summary>
public static class Paging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int? page, int? pageSize) =>
        (page ?? 1, Math.Min(pageSize ?? DefaultPageSize, MaxPageSize));
}
