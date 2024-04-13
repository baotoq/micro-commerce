using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using FluentValidation;
using Infrastructure;
using MediatR;

namespace Api.UseCases.Products;

public record CreateProductCommand : IRequest<CreateProductResponse>
{
    public static Func<IMediator, CreateProductCommand, Task<CreateProductResponse>> EndpointHandler => (mediator, request) => mediator.Send(request);
    
    public string Name { get; init; } = "";
    public decimal Price { get; set; }
    public int RemainingStock { get; set; }
}

public class CreateProductCommandHandler(ApplicationDbContext context, ILogger<CreateProductCommandHandler> logger, ElasticsearchClient esClient) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Products.AddAsync(new Product
        {
            Name = request.Name,
            Price = request.Price,
            RemainingStock = request.RemainingStock,
            TotalStock = request.RemainingStock,
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        
        var doc = new ProductDocument
        {
            Id = entity.Entity.Id,
            Name = entity.Entity.Name,
            Price = entity.Entity.Price,
        };

        var response = await esClient.IndexAsync(doc, cancellationToken);
            
        logger.LogInformation("Create product successfully");
            
        return new CreateProductResponse(entity.Entity.Id);
    }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.RemainingStock).GreaterThan(0);
    }
}

public record CreateProductResponse(string Id);