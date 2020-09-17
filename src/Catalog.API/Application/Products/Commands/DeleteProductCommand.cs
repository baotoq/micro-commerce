using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Data.UnitOfWork.EF.Core;
using MediatR;
using Shared.FileStorage;
using Shared.MediatR.Exceptions;

namespace Catalog.API.Application.Products.Commands
{
    public class DeleteProductCommand : IRequest
    {
        public long Id { get; set; }

        public DeleteProductCommand(long id)
        {
            Id = id;
        }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product> _repository;
        private readonly IStorageService _storageService;

        public DeleteProductCommandHandler(IUnitOfWork unitOfWork, IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Product>();
            _storageService = storageService;
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.FindAsync(request.Id, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException(nameof(Product), request.Id);
            }

            await _repository.RemoveAsync(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _storageService.DeleteAsync(product.ImageUri, cancellationToken);

            return Unit.Value;
        }
    }
}
