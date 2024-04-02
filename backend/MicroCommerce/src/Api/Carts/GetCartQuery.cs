using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Carts;

public class GetCartQuery : IRequest<GetCartQuery.Response>
{
    public string CartId { get; set; } = "";
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<GetCartQuery, Response>
    {
        public async Task<Response> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var cart = await context.Carts
                .Where(s => s.Id == request.CartId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }
            
            return new Response(cart.Id);
        }
    }

    public record Response(string CartId);
}