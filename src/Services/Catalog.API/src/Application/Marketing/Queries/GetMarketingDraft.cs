using System.Globalization;
using MediatR;
using MicroCommerce.Catalog.Application.Marketing.Dtos;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Marketing;
using MicroCommerce.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Marketing.Queries;

public record GetMarketingDraftQuery(Guid CampaignId) : IRequest<MarketingDraftDto?>;

public class GetMarketingDraftHandler(AppDbContext db) : IRequestHandler<GetMarketingDraftQuery, MarketingDraftDto?>
{
    public async Task<MarketingDraftDto?> Handle(GetMarketingDraftQuery request, CancellationToken ct)
    {
        var id = Domain.Marketing.CampaignId.From(request.CampaignId);
        var campaign = await db.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (campaign is null) return null;

        string? productName = null;
        string? productInventoryLabel = null;

        if (!string.IsNullOrWhiteSpace(campaign.FeaturedProductSku))
        {
            var sku = Sku.From(campaign.FeaturedProductSku);
            var product = await db.Products
                .AsNoTracking()
                .Where(p => p.Sku == sku)
                .Select(p => new { p.Name, p.Inventory, p.Price })
                .FirstOrDefaultAsync(ct);

            if (product is not null)
            {
                productName = product.Name;
                // Trim trailing zeros so a decimal(18,2) price of 86.00 renders as "$86"
                // (not "$86.00"), matching the design copy ("24 in stock · $86").
                var price = product.Price.ToString("0.##", CultureInfo.InvariantCulture);
                productInventoryLabel = $"{product.Inventory} in stock · ${price}";
            }
        }

        return new MarketingDraftDto(
            campaign.Id.Value,
            campaign.Subject,
            campaign.Subject.Length,
            campaign.AudienceKey,
            campaign.TemplateKey,
            campaign.FeaturedProductSku,
            productName,
            productInventoryLabel,
            MarketingMapping.AudienceOptions,
            MarketingMapping.TemplateOptions);
    }
}
