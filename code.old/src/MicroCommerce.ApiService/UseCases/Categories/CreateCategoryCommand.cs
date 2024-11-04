using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;

public class CreateCategoryCommand : IRequest<string>
{
    public string Name { get; set; }
    
    public class Handler(ApplicationDbContext context) : IRequestHandler<CreateCategoryCommand, string>
    {
        public async Task<string> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name
            };
            
            await context.Categories.AddAsync(category, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            return category.Id;
        }
    }
}