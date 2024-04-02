using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.UseCases.Carts;

public record IncreaseProductQuantityCommand : IRequest<IncreaseProductQuantityResponse>
{
    public string CartId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public int ProductQuantity { get; set; }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<IncreaseProductQuantityCommand, IncreaseProductQuantityResponse>
    {
        public async Task<IncreaseProductQuantityResponse> Handle(IncreaseProductQuantityCommand request, CancellationToken cancellationToken)
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

            if (cartProductMap == null)
            {
                cart.CartProductMaps.Add(new CartProductMap
                {
                    ProductId = product.Id,
                    CartId = cart.Id,
                    ProductQuantity = request.ProductQuantity
                });
            }
            else
            {
                cartProductMap.ProductQuantity += request.ProductQuantity;
            }

            await context.SaveChangesAsync(cancellationToken);
            await trans.CommitAsync(cancellationToken);
            
            return new IncreaseProductQuantityResponse(cart.Id);
        }
    }
}

public record IncreaseProductQuantityResponse(string CartId);