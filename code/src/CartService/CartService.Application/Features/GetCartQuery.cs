using Ardalis.GuardClauses;
using MediatR;
using MicroCommerce.CartService.Domain.Cart;
using MicroCommerce.CartService.Infrastructure;
using MicroCommerce.CartService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.CartService.Application.Features;

public class GetCartQuery : IRequest<CartDto>
{
    public required CartId CartId { get; set; }
}

public record CartDto
{
    public required CartId Id { get; init; }
}

public class GetCartQueryHandler(ApplicationDbContext _context) : IRequestHandler<GetCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(s => s.Id == request.CartId, cancellationToken: cancellationToken);

        if (cart is null)
        {
            throw new NotFoundException(request.CartId.ToString(), nameof(Cart));
        }

        var mapper = new CartMapper();

        return mapper.ToCartDto(cart);
    }
}
