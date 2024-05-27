using MediatR;
using MicroCommerce.ApiService.Infrastructure;

namespace MicroCommerce.ApiService.UseCases.Products;

public class DeleteProductCommand : IRequest<string>
{
    public DeleteProductCommand(string id)
    {
        Id = id;
    }
    
    public string Id { get; }
    
    public  class Handler(ApplicationDbContext context) : IRequestHandler<DeleteProductCommand, string>
    {
        public async Task<string> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await context.Products.FindAsync([request.Id], cancellationToken);
            
            if (product is null)
            {
                throw new Exception("Product not found");
            }
            
            context.Products.Remove(product);
            
            await context.SaveChangesAsync(cancellationToken);
            
            return product.Id;
        }
    }
}

