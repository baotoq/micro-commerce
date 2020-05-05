using MediatR;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data;

namespace BShop.API.Categories.Commands.Delete
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly ApplicationDbContext _context;

        public DeleteCategoryCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories.FindAsync(request.Id, cancellationToken);

            if (category == null)
            {
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
