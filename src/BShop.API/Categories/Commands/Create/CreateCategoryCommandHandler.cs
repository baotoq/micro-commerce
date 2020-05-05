using MediatR;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Categories.Models;
using BShop.API.Data;
using BShop.API.Data.Models;

namespace BShop.API.Categories.Commands.Create
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
    {
        private readonly ApplicationDbContext _context;

        public CreateCategoryCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new Category
            {
                Name = request.Name
            };

            await _context.Categories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
