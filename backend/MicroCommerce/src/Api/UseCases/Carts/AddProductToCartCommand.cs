using Domain.Entities;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.UseCases.Carts;

public record AddProductToCartCommand : IRequest<AddProductToCartResponse>
{
    public string CartId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public int ProductQuantity { get; set; }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<AddProductToCartCommand, AddProductToCartResponse>
    {
        public async Task<AddProductToCartResponse> Handle(AddProductToCartCommand request, CancellationToken cancellationToken)
        {
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
            
            cart.CartProductMaps.Add(new CartProductMap
            {
                ProductId = product.Id,
                CartId = cart.Id,
                ProductQuantity = request.ProductQuantity
            });
            
            return new AddProductToCartResponse(cart.Id);
        }
    }
}

public record AddProductToCartResponse(string CartId);