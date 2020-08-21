using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Data.UnitOfWork.EF;
using MediatR;
using Shared.MediatR.Exceptions;

namespace Catalog.API.Application.Categories.Commands
{
    public class UpdateCategoryCommand : IRequest
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        private readonly IEfUnitOfWork _unitOfWork;
        private readonly IRepository<Category> _repository;

        public UpdateCategoryCommandHandler(IEfUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Category>();
        }

        public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
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
