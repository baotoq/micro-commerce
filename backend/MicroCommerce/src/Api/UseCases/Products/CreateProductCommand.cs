using Domain.Entities;
using Infrastructure;
using MediatR;

namespace Api.UseCases.Products;

public record CreateProductCommand(string Name) : IRequest<CreateProductResponse>
{
    public class Handler(ApplicationDbContext context) : IRequestHandler<CreateProductCommand, CreateProductResponse>
    {
        public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var entity = await context.Products.AddAsync(new Product
            {
                Name = request.Name
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            
            return new CreateProductResponse(entity.Entity.Id);
        }
    }
}

public record CreateProductResponse(string Id);