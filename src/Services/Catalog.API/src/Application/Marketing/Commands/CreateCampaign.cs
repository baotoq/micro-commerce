using MediatR;
using MicroCommerce.Catalog.Application.Marketing.Dtos;
using MicroCommerce.Catalog.Application.Marketing.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Marketing;

namespace MicroCommerce.Catalog.Application.Marketing.Commands;

public record CampaignFollowupInput(
    bool Enabled,
    string Kind,
    int DelayDays,
    string Subject,
    string Preview);

public record CreateCampaignCommand(
    string Name,
    string Subject,
    string PreviewText,
    string AudienceKey,
    string TemplateKey,
    string? FeaturedProductSku,
    bool FollowupEnabled,
    CampaignFollowupInput? Followup,
    IClock Clock) : IRequest<Result<CampaignDto>>;

public class CreateCampaignHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<CreateCampaignCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(CreateCampaignCommand request, CancellationToken ct)
    {
        CampaignFollowup? followup = request.Followup is null
            ? null
            : new CampaignFollowup(
                request.Followup.Enabled,
                MarketingMapping.ParseKind(request.Followup.Kind),
                request.Followup.DelayDays,
                request.Followup.Subject,
                request.Followup.Preview);

        var campaign = new Campaign(
            request.Name,
            request.Subject,
            request.PreviewText,
            request.AudienceKey,
            request.TemplateKey,
            request.FollowupEnabled,
            followup,
            request.FeaturedProductSku,
            request.Clock.Now);

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(ct);

        var dto = MarketingMapping.ToDto(campaign);
        await publisher.Publish(new CampaignCreatedEvent(dto.Id), ct);
        return Result<CampaignDto>.Success(dto);
    }
}
