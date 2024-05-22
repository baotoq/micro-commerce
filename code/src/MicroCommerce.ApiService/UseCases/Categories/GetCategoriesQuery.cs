using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Categories;

public record GetCategoriesQuery : IRequest<IEnumerable<CategoryViewModel>>
{
    public static Func<IMediator, Task<IEnumerable<CategoryViewModel>>> EndpointHandler => (mediator) => mediator.Send(new GetCategoriesQuery());
}

public class GetProductQueryHandler(ApplicationDbContext context) : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryViewModel>>
{
    public async Task<IEnumerable<CategoryViewModel>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .ToListAsync(cancellationToken);

        return categories.ConvertAll(s => new CategoryViewModel(s));
    }
}

public record CategoryViewModel
{
    public CategoryViewModel(Category domain)
    {
        Id = domain.Id;
        Name = domain.Name;
    }
    
    public string Id { get; set; }

    public string Name { get; set; }
};