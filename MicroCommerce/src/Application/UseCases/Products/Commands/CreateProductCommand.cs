using Application.Common;
using Application.UseCases.Products.Events;
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
        private readonly IPublishEndpoint _publishEndpoint;
        
        public Handler(ApplicationDbContext context, IPublishEndpoint publishEndpoint) : base(context)
        {
            _publishEndpoint = publishEndpoint;
        }

        public override async Task<Response> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
        {
            // var added = await Context.Products.AddAsync(new Product
            // {
            //     Name = request.Name
            // }, cancellationToken);
            //
            // await Context.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ProductCreatedEvent
            {
                Id = "added.Entity.Id",
                Name = "added.Entity.Name"
            }, CancellationToken.None);
            
            return new Response
            {
                Name = "added.Entity.Name"
            };
        }
    }
}