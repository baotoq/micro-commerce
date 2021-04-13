using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MediatR;
using MicroCommerce.Shared.EventBus.Models;
using MicroCommerce.Shared.FileStorage;

namespace MicroCommerce.Catalog.API.IntegrationEvents.Events
{
    public record ProductUpdated : IntegrationEvent, IRequest<Result>
    {
        public ProductUpdated(string imageUri)
        {
            ImageUri = imageUri;
        }

        public string ImageUri { get; init; }
    }

    public class ProductUpdatedHandler : IRequestHandler<ProductUpdated, Result>
    {
        private readonly IStorageService _storageService;

        public ProductUpdatedHandler(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<Result> Handle(ProductUpdated request, CancellationToken cancellationToken)
        {
            await _storageService.DeleteAsync(request.ImageUri, cancellationToken);

            return Result.Success(true);
        }
    }
}
