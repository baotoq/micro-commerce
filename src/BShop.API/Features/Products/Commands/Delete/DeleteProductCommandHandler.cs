using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data;
using BShop.API.Data.Models;
using MediatR;

namespace BShop.API.Features.Products.Commands.Delete
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IRepository<Product> _repository;

        public DeleteProductCommandHandler(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.FindAsync(cancellationToken, request.Id);

            if (product == null)
            {
                return false;
            }

            _repository.Remove(product);
            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
