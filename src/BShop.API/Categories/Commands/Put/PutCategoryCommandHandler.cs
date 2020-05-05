using System.Threading;
using System.Threading.Tasks;
using BShop.API.Categories.Models;
using BShop.API.Data;
using BShop.API.Data.Models;
using MediatR;

namespace BShop.API.Categories.Commands.Put
{
    public class PutCategoryCommandHandler : IRequestHandler<PutCategoryCommand, CategoryDto>
    {
        private readonly ApplicationDbContext _context;

        public PutCategoryCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDto> Handle(PutCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories.FindAsync(request.Id);

            if (category == null)
            {
                return null;
            }

            category.Name = request.Name;
            await _context.SaveChangesAsync(cancellationToken);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
