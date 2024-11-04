using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Categories;

public record GetCategoryQuery : IRequest<CategoryViewModel>
{
    public GetCategoryQuery(string id)
    {
        Id = id;
    }
    
    public string Id { get; } = "";
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<GetCategoryQuery, CategoryViewModel>
    {
        public async Task<CategoryViewModel> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            var category = await context.Categories.FindAsync([request.Id], cancellationToken);
            
            if (category is null)
            {
                throw new Exception("Category not found");
            }
            
            return new CategoryViewModel(category);
        }
    }
}
