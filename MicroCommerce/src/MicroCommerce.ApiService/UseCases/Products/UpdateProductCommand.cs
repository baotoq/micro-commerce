using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Products;

public record UpdateProductCommand : IRequest<UpdateProductResponse>
{
    public static Func<IMediator, UpdateProductCommand, Task<UpdateProductResponse>> EndpointHandler => (mediator, request) => mediator.Send(request);
    
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public decimal Price { get; init; }
    public int RemainingStock { get; init; }
}

public class UpdateProductCommandHandler(ApplicationDbContext context, ILogger<UpdateProductCommandHandler> logger) : IRequestHandler<UpdateProductCommand, UpdateProductResponse>
{
    public async Task<UpdateProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (product == null)
        {
            throw new Exception("Not found");
        }
        
        product.Name = request.Name;
        product.Price = request.Price;
        product.RemainingStock = request.RemainingStock;
        product.TotalStock = request.RemainingStock;
        
        await context.SaveChangesAsync(cancellationToken);
            
        logger.LogInformation("Update product successfully");
            
        return new UpdateProductResponse(product.Id);
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.RemainingStock).GreaterThan(0);
    }
}

public record UpdateProductResponse(string Id);