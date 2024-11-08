using Elastic.Clients.Elasticsearch;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Features.DomainEvents;
using MicroCommerce.ApiService.Infrastructure;

namespace MicroCommerce.ApiService.Features;

public record CreateProductCommand : IRequest<CreateProductResponse>
{
    public string Name { get; init; } = "";
    public decimal Price { get; set; }
    public int RemainingStock { get; set; }
}

public class CreateProductCommandHandler(ApplicationDbContext context, ILogger<CreateProductCommandHandler> logger, ElasticsearchClient esClient) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            RemainingStock = request.RemainingStock,
            TotalStock = request.RemainingStock,
        };

        var entity = await context.Products.AddAsync(product, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

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

public record CreateProductResponse(Guid Id);
