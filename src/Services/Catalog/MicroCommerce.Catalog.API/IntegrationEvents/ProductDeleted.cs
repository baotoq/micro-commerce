using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MediatR;
using MicroCommerce.Shared.EventBus;
using MicroCommerce.Shared.FileStorage;

namespace MicroCommerce.Catalog.API.IntegrationEvents
{
    public record ProductDeleted : IntegrationEvent, IRequest<Result>
    {
        public ProductDeleted(string imageUri)
        {
            ImageUri = imageUri;
        }

        public string ImageUri { get; init; }
    }

    public class ProductDeletedHandler : IRequestHandler<ProductDeleted, Result>
    {
        private readonly IStorageService _storageService;

        public ProductDeletedHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<Result> Handle(ProductDeleted request, CancellationToken cancellationToken)
        {
            await _storageService.DeleteAsync(request.ImageUri, cancellationToken);

            return Result.Success(true);
        }
    }
}
