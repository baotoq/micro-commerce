using Application.Common;
using Application.UseCases.Products.DomainEvents;
using Domain.Entities;
using Infrastructure.Persistence;
using MassTransit;
using MediatR;

namespace Application.UseCases.Products.Commands;

public class CreateProductCommand : IRequest<CreateProductCommand.Response>
{
    public string Name { get; set; } = string.Empty;
    
    public class Response
    {
        public string Name { get; set; } = string.Empty;
    }
    
    public class Handler : RequestHandlerBase<CreateProductCommand, Response>
    {
        public Handler(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Response> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
        {
            var product = new Product
            {
                Name = request.Name
            };
            
            var added = await Context.Products.AddAsync(product, cancellationToken);
            
            product.AddDomainEvent(new ProductCreatedDomainEvent
            {
                Id = added.Entity.Id,
                Name = added.Entity.Name
            });
            
            await Context.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Name = added.Entity.Name
            };
        }
    }
}