using Application.Common;
using Application.UseCases.Carts.DomainEvents;
using Application.UseCases.Products.DomainEvents;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.Carts.Commands;

public class AddProductToCartCommand : IRequest<string>
{
    public string CartId { get; set; } = "";
    public string ProductId { get; set; } = "";
    public int Quantities { get; set; }
    
    public class Handler : RequestHandlerBase<AddProductToCartCommand, string>
    {
        public Handler(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<string> Handle(AddProductToCartCommand request, CancellationToken cancellationToken = default)
        {
            var cart = await Context.Carts.Where(s => s.Id == request.CartId).FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                return "";
            }
            
            var product = await Context.Products.Where(p => p.Id == request.ProductId).FirstOrDefaultAsync(cancellationToken);

            if (product is null)
            {
                return "";
            }
            
            cart.AddProduct(product, request.Quantities);
            cart.AddDomainEvent(new ProductAddedToCartDomainEvent
            {
                CartId = cart.Id,
                ProductId = cart.Id,
                Quantities = request.Quantities,
            });

            await Context.SaveChangesAsync(cancellationToken);

            return "";
        }
    }
}