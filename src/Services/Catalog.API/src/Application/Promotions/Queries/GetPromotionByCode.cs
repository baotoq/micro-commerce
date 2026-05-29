using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Promotions.Dtos;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Promotions.Queries;

public record GetPromotionByCodeQuery(string Code) : IRequest<PromotionDto?>;

public class GetPromotionByCodeHandler(AppDbContext db) : IRequestHandler<GetPromotionByCodeQuery, PromotionDto?>
{
    public async Task<PromotionDto?> Handle(GetPromotionByCodeQuery request, CancellationToken ct)
    {
        var code = PromotionCode.From(request.Code);
        return await db.Promotions
            .AsNoTracking()
            .Where(p => p.Code == code)
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
            .FirstOrDefaultAsync(ct);
    }
}

public record PromotionExistsByCodeQuery(string Code) : IRequest<bool>;

public class PromotionExistsByCodeHandler(AppDbContext db) : IRequestHandler<PromotionExistsByCodeQuery, bool>
{
    public async Task<bool> Handle(PromotionExistsByCodeQuery request, CancellationToken ct)
    {
        var code = PromotionCode.From(request.Code);
        return await db.Promotions.AsNoTracking().AnyAsync(p => p.Code == code, ct);
    }
}
