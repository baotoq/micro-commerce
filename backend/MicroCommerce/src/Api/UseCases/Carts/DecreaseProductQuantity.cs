using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.UseCases.Carts;

public record DecreaseProductQuantityCommand : IRequest<DecreaseProductQuantityResponse>
{
    public string CartId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public int ProductQuantity { get; set; }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<DecreaseProductQuantityCommand, DecreaseProductQuantityResponse>
    {
        public async Task<DecreaseProductQuantityResponse> Handle(DecreaseProductQuantityCommand request, CancellationToken cancellationToken)
        {
            await using var trans = await context.Database.BeginTransactionAsync(cancellationToken);
            
            var cart = await context.Carts
                .Where(s => s.Id == request.CartId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                throw new Exception("Cart not found");
            }
            
            var product = await context.Products
                .Where(s => s.Id == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
            {
                throw new Exception("Product not found");
            }
            
            var cartProductMap = await context.CartProductMaps
                .Where(s => s.CartId == request.CartId && s.ProductId == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            if (cartProductMap != null)
            {
                cartProductMap.ProductQuantity -= request.ProductQuantity;

                if (cartProductMap.ProductQuantity == 0)
                {
                    context.CartProductMaps.Remove(cartProductMap);
                }
                
                product.RemainingStock += request.ProductQuantity;
            }
            
            await context.SaveChangesAsync(cancellationToken);
            await trans.CommitAsync(cancellationToken);
            
            return new DecreaseProductQuantityResponse(cart.Id);
        }
    }
}

public record DecreaseProductQuantityResponse(string CartId);