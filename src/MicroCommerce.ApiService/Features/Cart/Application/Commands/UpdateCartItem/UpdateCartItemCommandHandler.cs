using FluentResults;
using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Cart.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Cart.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.UpdateCartItem;

public sealed class UpdateCartItemCommandHandler(CartDbContext context)
    : IRequestHandler<UpdateCartItemCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCartItemCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Cart? cart = await context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.BuyerId == request.BuyerId, cancellationToken)
            ?? throw new NotFoundException($"Cart not found for buyer '{request.BuyerId}'.");

        try
        {
            cart.UpdateItemQuantity(CartItemId.From(request.ItemId), request.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
