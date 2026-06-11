using MediatR;
using MicroCommerce.Catalog.Application.Marketing.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Products.Dtos;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Marketing.Queries;

public record GetCampaignsQuery(int Page, int PageSize, string? Status) : IRequest<PagedResult<CampaignDto>>;

public class GetCampaignsHandler(AppDbContext db) : IRequestHandler<GetCampaignsQuery, PagedResult<CampaignDto>>
{
    public async Task<PagedResult<CampaignDto>> Handle(GetCampaignsQuery request, CancellationToken ct)
    {
        var query = db.Campaigns.AsNoTracking();

        // Frontend fetches the single draft via ?status=draft&limit=1 (plan OQ5).
        if (MarketingMapping.TryParseStatus(request.Status) is { } status)
            query = query.Where(c => c.Status == status);

        var total = await query.CountAsync(ct);
        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = campaigns.Select(MarketingMapping.ToDto).ToList();

        return new PagedResult<CampaignDto>(items, total, request.Page, request.PageSize);
    }
}
