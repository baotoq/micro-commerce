using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Carts;

public record GetCartQuery(string CartId) : IRequest<GetCartResponse>
{
    public class Handler(ApplicationDbContext context) : IRequestHandler<GetCartQuery, GetCartResponse>
    {
        public async Task<GetCartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var cart = await context.Carts
                .Where(s => s.Id == request.CartId)
                .FirstOrDefaultAsync(cancellationToken);
 
            if (cart == null)
            {
                throw new Exception("Cart not found");
            }

            return new GetCartResponse(cart.Id);
        }
    }
}

public record GetCartResponse(string CartId);