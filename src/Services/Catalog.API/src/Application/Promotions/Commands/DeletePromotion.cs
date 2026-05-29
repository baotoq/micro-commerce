using MediatR;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Application.Promotions.Events;
using MicroCommerce.Catalog.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Promotions.Commands;

public record DeletePromotionCommand(string Code) : IRequest<bool>;

public class DeletePromotionHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<DeletePromotionCommand, bool>
{
    public async Task<bool> Handle(DeletePromotionCommand request, CancellationToken ct)
    {
        var code = PromotionCode.From(request.Code);
        var promotion = await db.Promotions.FirstOrDefaultAsync(p => p.Code == code, ct);
        if (promotion is null) return false;

        db.Promotions.Remove(promotion);
        await db.SaveChangesAsync(ct);
        await publisher.Publish(new PromotionDeletedEvent(code.Value), ct);
        return true;
    }
}
