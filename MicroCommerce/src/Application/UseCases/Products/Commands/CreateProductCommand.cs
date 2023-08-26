using Application.Common;
using Application.UseCases.Carts.Queries;
using Domain.Entities;
using Infrastructure.Persistence;
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
            await Context.Products.AddAsync(new Product
            {
                Name = request.Name
            }, cancellationToken);

            await Context.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Name = request.Name
            };
        }
    }
}