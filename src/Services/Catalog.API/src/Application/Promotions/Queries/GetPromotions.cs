using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using MicroCommerce.Catalog.Application.Promotions.Dtos;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Promotions.Queries;

public record GetPromotionsQuery(int Page, int PageSize, string? Status) : IRequest<PagedResult<PromotionDto>>;

public class GetPromotionsHandler(AppDbContext db) : IRequestHandler<GetPromotionsQuery, PagedResult<PromotionDto>>
{
    public async Task<PagedResult<PromotionDto>> Handle(GetPromotionsQuery request, CancellationToken ct)
    {
        var query = db.Promotions.AsNoTracking();

        if (PromotionMapping.TryParseStatus(request.Status) is { } status)
            query = query.Where(p => p.Status == status);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Code)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PromotionDto(
                p.Code.Value,
                p.Description,
                p.Kind == DiscountKind.Percentage ? "percentage" : "fixed",
                p.PercentValue,
                p.FixedAmount,
                p.MinOrderAmount,
                p.StartsAt,
                p.EndsAt,
                p.Status == PromotionStatus.Active ? "active"
                    : p.Status == PromotionStatus.Ended ? "ended"
                    : "draft"))
            .ToListAsync(ct);

        return new PagedResult<PromotionDto>(items, total, request.Page, request.PageSize);
    }
}
