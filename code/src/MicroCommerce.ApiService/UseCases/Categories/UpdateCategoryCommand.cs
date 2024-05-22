using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;

public class UpdateCategoryCommand : IRequest<string>
{
    public string Id { get; set; }
    public string Name { get; set; }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<UpdateCategoryCommand, string>
    {
        public async Task<string> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await context.Categories.FindAsync(request.Id);
            
            if (category is null)
            {
                throw new Exception("Category not found");
            }
            
            category.Name = request.Name;
            
            await context.SaveChangesAsync(cancellationToken);
            
            return category.Id;
        }
    }
}