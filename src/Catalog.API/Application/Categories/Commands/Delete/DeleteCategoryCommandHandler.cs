using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data;
using Catalog.API.Data.Models;
using Catalog.API.Infrastructure.Exceptions;
using MediatR;

namespace Catalog.API.Application.Categories.Commands.Delete
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Category> _repository;

        public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Category>();
        }

        public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.FindAsync(request.Id, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException(nameof(Category), request.Id);
            }

            _repository.Remove(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
