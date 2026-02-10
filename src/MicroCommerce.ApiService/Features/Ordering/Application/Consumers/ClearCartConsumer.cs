using MassTransit;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using MicroCommerce.ApiService.Features.Ordering.Application.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Consumers;

/// <summary>
/// Clears the buyer's cart after a successful checkout.
/// Uses ExecuteDeleteAsync for efficiency (follows cart expiration pattern).
/// </summary>
public sealed class ClearCartConsumer(
    CartDbContext cartDb,
    ILogger<ClearCartConsumer> logger) : IConsumer<ClearCart>
{
    public async Task Consume(ConsumeContext<ClearCart> context)
    {
        // Delete cart items first (cascade might handle this, but explicit is safer)
        Cart.Domain.Entities.Cart? cart = await cartDb.Carts
            .FirstOrDefaultAsync(c => c.BuyerId == context.Message.BuyerId, context.CancellationToken);

        if (cart is null)
        {
            logger.LogInformation("No cart found for buyer {BuyerId}, nothing to clear", context.Message.BuyerId);
            return;
        }

        // Delete cart items for this cart
        await cartDb.CartItems
            .Where(ci => ci.CartId == cart.Id)
            .ExecuteDeleteAsync(context.CancellationToken);

        // Delete the cart itself
        cartDb.Carts.Remove(cart);
        await cartDb.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Cart cleared for buyer {BuyerId} after successful checkout", context.Message.BuyerId);
    }
}
