using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Promotions.Dtos;
using MicroCommerce.Catalog.Application.Promotions.Events;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Promotions.Commands;

public record UpdatePromotionCommand(
    string Code,
    string Description,
    string Kind,
    int? PercentValue,
    decimal? FixedAmount,
    decimal? MinOrderAmount,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt) : IRequest<PromotionDto?>;

public class UpdatePromotionHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<UpdatePromotionCommand, PromotionDto?>
{
    public async Task<PromotionDto?> Handle(UpdatePromotionCommand request, CancellationToken ct)
    {
        var code = PromotionCode.From(request.Code);
        // AsTracking() is required: DbContext default is NoTracking, so mutating the loaded
        // entity without it would make SaveChanges a silent no-op
        // (project memory: project_ef_notracking_mutating_handlers).
        var promotion = await db.Promotions
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Code == code, ct);
        if (promotion is null) return null;
        if (promotion.Status == PromotionStatus.Ended) return null;

        promotion.UpdateDetails(
            request.Description,
            PromotionMapping.ParseKind(request.Kind),
            request.PercentValue,
            request.FixedAmount,
            request.MinOrderAmount,
            request.StartsAt,
            request.EndsAt);

        await db.SaveChangesAsync(ct);

        var dto = PromotionMapping.ToDto(promotion);
        await publisher.Publish(new PromotionUpdatedEvent(dto.Code, dto.Status), ct);
        return dto;
    }
}
