using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Promotions.Dtos;
using MicroCommerce.Catalog.Application.Promotions.Events;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.Catalog.Application.Promotions.Commands;

public record CreatePromotionCommand(
    string Code,
    string Description,
    string Kind,
    int? PercentValue,
    decimal? FixedAmount,
    decimal? MinOrderAmount,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool ActivateImmediately) : IRequest<Result<PromotionDto>>;

public class CreatePromotionHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<CreatePromotionCommand, Result<PromotionDto>>
{
    private const string PostgresUniqueViolation = "23505";

    public async Task<Result<PromotionDto>> Handle(CreatePromotionCommand request, CancellationToken ct)
    {
        var code = PromotionCode.From(request.Code);
        var kind = PromotionMapping.ParseKind(request.Kind);

        // Optimistic pre-check; unique index on Code is the authoritative guarantee
        // via the DbUpdateException catch below.
        if (await db.Promotions.AsNoTracking().AnyAsync(p => p.Code == code, ct))
            return Result<PromotionDto>.Conflict("PROMOTION_CODE_DUPLICATE", $"Promotion with code '{code.Value}' already exists.");

        var promotion = new Promotion(
            code,
            request.Description,
            kind,
            request.PercentValue,
            request.FixedAmount,
            request.MinOrderAmount,
            request.StartsAt,
            request.EndsAt);

        // Atomic create-and-activate: call domain Activate() before the first SaveChanges
        // so there is no orphan-Draft path.
        if (request.ActivateImmediately)
            promotion.Activate();

        db.Promotions.Add(promotion);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresUniqueViolation })
        {
            return Result<PromotionDto>.Conflict("PROMOTION_CODE_DUPLICATE", $"Promotion with code '{code.Value}' already exists.");
        }

        var dto = PromotionMapping.ToDto(promotion);
        await publisher.Publish(new PromotionCreatedEvent(dto.Code, dto.Status), ct);
        return Result<PromotionDto>.Success(dto);
    }
}
