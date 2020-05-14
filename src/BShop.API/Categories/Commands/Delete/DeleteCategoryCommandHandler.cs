using MediatR;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data;
using BShop.API.Data.Models;

namespace BShop.API.Categories.Commands.Delete
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly IRepository<Category> _repository;

        public DeleteCategoryCommandHandler(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.FindAsync(cancellationToken, request.Id);

            if (category == null)
            {
                return false;
            }

            _repository.Remove(category);
            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
