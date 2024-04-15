using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Database;

public record SeedDataCommand : IRequest<SeedDataResponse>
{
    public static Func<IMediator, Task<SeedDataResponse>> EndpointHandler => mediator => mediator.Send(new SeedDataCommand());
}

public class SeedDataCommandHandler(ApplicationDbContext context) : IRequestHandler<SeedDataCommand, SeedDataResponse>
{
    private readonly IList<string> _categories = new List<string>()
    {
        "Phone", "Cloth", "Shoes", "Watches", "Laptop"
    };
        
    public async Task<SeedDataResponse> Handle(SeedDataCommand request, CancellationToken cancellationToken)
    {
        if (!await context.Categories.AnyAsync(cancellationToken))
        {
            foreach (var category in _categories)
            {
                await context.Categories.AddAsync(new Category
                {
                    Name = category
                }, cancellationToken);
            }
        }
        
        if (!await context.Products.AnyAsync(cancellationToken))
        {
            var products = new List<Product>()
            {
                new()
                {
                    Name = "Apple",
                    Price = 20
                },
                new()
                {
                    Name = "Iphone",
                    Price = 30
                },
                new()
                {
                    Name = "Macbook",
                    Price = 200
                }
            };
            await context.Products.AddRangeAsync(products, cancellationToken);
        }
            
        await context.SaveChangesAsync(cancellationToken);
            
        return new SeedDataResponse();
    }
}

public record SeedDataResponse();