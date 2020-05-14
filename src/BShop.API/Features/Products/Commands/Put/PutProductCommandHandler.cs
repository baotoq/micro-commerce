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
        private readonly IRepository<Product> _repository;

        public PutProductCommandHandler(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<ProductDto> Handle(PutProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.FindAsync(cancellationToken, request.Id);

            if (product == null)
            {
                return null;
            }

            product.Name = request.Name;
            await _repository.SaveChangesAsync(cancellationToken);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name
            };
        }
    }
}
