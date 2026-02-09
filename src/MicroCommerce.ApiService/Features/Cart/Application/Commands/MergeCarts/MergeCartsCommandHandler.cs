using MediatR;
using Microsoft.EntityFrameworkCore;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.MergeCarts;

public sealed class MergeCartsCommandHandler(CartDbContext context) : IRequestHandler<MergeCartsCommand>
{
    public async Task Handle(MergeCartsCommand request, CancellationToken cancellationToken)
    {
        // Load guest cart with items
        var guestCart = await context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.GuestBuyerId, cancellationToken);

        // No guest cart or empty cart - nothing to merge
        if (guestCart is null || !guestCart.Items.Any())
        {
            return;
        }

        // Load authenticated user's cart
        var authCart = await context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.AuthenticatedBuyerId, cancellationToken);

        // If no auth cart exists, transfer ownership of guest cart
        if (authCart is null)
        {
            guestCart.TransferOwnership(request.AuthenticatedBuyerId);
        }
        else
        {
            // Merge guest items into auth cart
            foreach (var guestItem in guestCart.Items)
            {
                authCart.AddItem(
                    guestItem.ProductId,
                    guestItem.ProductName,
                    guestItem.UnitPrice,
                    guestItem.ImageUrl,
                    guestItem.Quantity);
            }

            // Delete guest cart after merge
            context.Carts.Remove(guestCart);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
