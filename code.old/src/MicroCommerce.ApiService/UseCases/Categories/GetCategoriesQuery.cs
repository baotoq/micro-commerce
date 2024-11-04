using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Categories;

public record GetCategoriesQuery : IRequest<IEnumerable<CategoryViewModel>>
{
    public class Handler(ApplicationDbContext context) : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryViewModel>>
    {
        public async Task<IEnumerable<CategoryViewModel>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await context.Categories
                .ToListAsync(cancellationToken);
            
            return categories.ConvertAll(s => new CategoryViewModel(s));
        }
    }
}