using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Promotions.Dtos;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Promotions.Queries;

public record GetPromotionStatsQuery : IRequest<PromotionStatsDto>;

public class GetPromotionStatsHandler(AppDbContext db) : IRequestHandler<GetPromotionStatsQuery, PromotionStatsDto>
{
    public async Task<PromotionStatsDto> Handle(GetPromotionStatsQuery request, CancellationToken ct)
    {
        var promotions = db.Promotions.AsNoTracking();

        var counts = await promotions
            .GroupBy(_ => 1)
            .Select(g => new PromotionStatsDto(
                g.Count(p => p.Status == PromotionStatus.Active),
                g.Count(p => p.Status == PromotionStatus.Draft),
                g.Count(p => p.Status == PromotionStatus.Ended),
                g.Count()))
            .FirstOrDefaultAsync(ct);

        return counts ?? new PromotionStatsDto(0, 0, 0, 0);
    }
}
