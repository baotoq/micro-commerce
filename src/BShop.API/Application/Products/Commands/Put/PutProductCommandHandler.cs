using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data;
using BShop.API.Data.Models;
using BShop.API.Infrastructure.Exceptions;
using MediatR;

namespace BShop.API.Application.Products.Commands.Put
{
    public class PutProductCommandHandler : IRequestHandler<PutProductCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product> _repository;

        public PutProductCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Product>();
        }


        public async Task<Unit> Handle(PutProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.FindAsync(request.Id, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException(nameof(Product), request.Id);
            }

            product.Name = request.Name;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
