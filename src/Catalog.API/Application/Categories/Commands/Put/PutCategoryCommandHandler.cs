using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using MediatR;
using Shared.MediatR.Exceptions;
using UnitOfWork;

namespace Catalog.API.Application.Categories.Commands.Put
{
    public class PutCategoryCommandHandler : IRequestHandler<PutCategoryCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Category> _repository;

        public PutCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Category>();
        }

        public async Task<Unit> Handle(PutCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.FindAsync(request.Id, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException(nameof(Category), request.Id);
            }

            category.Name = request.Name;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
