using MediatR;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.UseCases.Categories;

public record DeleteCategoryCommand : IRequest<string>
{
    public DeleteCategoryCommand(string id)
    {
        Id = id;
    }
    
    public string Id { get; } = "";
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<DeleteCategoryCommand, string>
    {
        public async Task<string> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await context.Categories.FindAsync([request.Id], cancellationToken);
            
            if (category is null)
            {
                throw new Exception("Category not found");
            }
            
            context.Categories.Remove(category);
            
            await context.SaveChangesAsync(cancellationToken);
            
            return category.Id;
        }
    }
}
