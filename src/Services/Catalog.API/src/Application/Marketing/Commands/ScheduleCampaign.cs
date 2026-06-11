using MediatR;
using MicroCommerce.Catalog.Application.Marketing.Dtos;
using MicroCommerce.Catalog.Application.Marketing.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Marketing;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Marketing.Commands;

public record ScheduleCampaignCommand(Guid Id, DateTimeOffset At) : IRequest<Result<CampaignDto>>;

public class ScheduleCampaignHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<ScheduleCampaignCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(ScheduleCampaignCommand request, CancellationToken ct)
    {
        var id = CampaignId.From(request.Id);
        // AsTracking() is required: DbContext default is NoTracking, so mutating the loaded
        // entity without it would make SaveChanges a silent no-op
        // (project memory: project_ef_notracking_mutating_handlers).
        var campaign = await db.Campaigns
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (campaign is null)
            return Result<CampaignDto>.Conflict("CAMPAIGN_NOT_FOUND", $"Campaign '{request.Id}' was not found.");

        try
        {
            campaign.Schedule(request.At);
        }
        catch (InvalidOperationException ex)
        {
            return Result<CampaignDto>.Conflict("CAMPAIGN_INVALID_TRANSITION", ex.Message);
        }

        await db.SaveChangesAsync(ct);

        var dto = MarketingMapping.ToDto(campaign);
        await publisher.Publish(new CampaignUpdatedEvent(dto.Id), ct);
        return Result<CampaignDto>.Success(dto);
    }
}
