using Domain.Entities;
using Infrastructure;
using MediatR;

namespace Api.UseCases.Products;

public record CreateProductCommand(string Name) : IRequest<CreateProductResponse>
{
    public static Func<IMediator, CreateProductCommand, Task<CreateProductResponse>> EndpointHandler => (mediator, request) => mediator.Send(request);
}

public class CreateProductCommandHandler(ApplicationDbContext context, ILogger<CreateProductCommandHandler> logger) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Products.AddAsync(new Product
        {
            Name = request.Name
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
            
        logger.LogInformation("Create product successfully");
            
        return new CreateProductResponse(entity.Entity.Id);
    }
}

public record CreateProductResponse(string Id);