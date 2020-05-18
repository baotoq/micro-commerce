using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data;
using BShop.API.Data.Models;
using BShop.API.Features.Products.Models;
using MediatR;

namespace BShop.API.Features.Products.Commands.Put
{
    public class PutProductCommandHandler : IRequestHandler<PutProductCommand, ProductDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product> _repository;

        public PutProductCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Product>();
        }

        public async Task<ProductDto> Handle(PutProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.FindAsync(cancellationToken, request.Id);

            if (product == null)
            {
                return null;
            }

            product.Name = request.Name;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name
            };
        }
    }
}
