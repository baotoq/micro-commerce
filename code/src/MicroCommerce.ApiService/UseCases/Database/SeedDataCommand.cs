using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Bogus;
    
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

        context.Products.RemoveRange(await context.Products.ToListAsync());
        context.SaveChanges();
        if (!await context.Products.AnyAsync(cancellationToken))
        {
            var faker = new Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(p => p.RemainingStock, f => f.Random.Number(0, 100))
                .RuleFor(p => p.TotalStock, f => f.Random.Number(100, 1000))
                .RuleFor(p => p.SoldQuantity, f => f.Random.Number(0, 100))
                .RuleFor(p => p.ImageUrl, f => f.Image.PicsumUrl());

            var products = faker.Generate(10);
            
            await context.Products.AddRangeAsync(products, cancellationToken);
        }
            
        await context.SaveChangesAsync(cancellationToken);
            
        return new SeedDataResponse();
    }
}

public record SeedDataResponse();