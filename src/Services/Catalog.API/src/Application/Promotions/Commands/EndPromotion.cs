using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Promotions.Dtos;
using MicroCommerce.Catalog.Application.Promotions.Events;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Promotions.Commands;

public record EndPromotionCommand(string Code) : IRequest<Result<PromotionDto>>;

public class EndPromotionHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<EndPromotionCommand, Result<PromotionDto>>
{
    public async Task<Result<PromotionDto>> Handle(EndPromotionCommand request, CancellationToken ct)
    {
        var code = PromotionCode.From(request.Code);
        var promotion = await db.Promotions
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Code == code, ct);
        if (promotion is null)
            return Result<PromotionDto>.Conflict("PROMOTION_NOT_FOUND", $"Promotion '{code.Value}' was not found.");

        try
        {
            promotion.End();
        }
        catch (InvalidOperationException ex)
        {
            return Result<PromotionDto>.Conflict("PROMOTION_INVALID_TRANSITION", ex.Message);
        }

        await db.SaveChangesAsync(ct);

        var dto = PromotionMapping.ToDto(promotion);
        await publisher.Publish(new PromotionStatusChangedEvent(dto.Code, dto.Status), ct);
        return Result<PromotionDto>.Success(dto);
    }
}
