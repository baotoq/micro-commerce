using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using MediatR;
using MicroCommerce.Catalog.API.IntegrationEvents;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Shared.EventBus.Abstractions;
using MicroCommerce.Shared.MediatR.Exceptions;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public record DeleteProductCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
    {
        private readonly ApplicationDbContext _context;
        private readonly IEventBus _eventBus;

        public DeleteProductCommandHandler(ApplicationDbContext context, IEventBus eventBus)
        {
            _context = context;
            _eventBus = eventBus;
        }

        public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var result = await Result.Try(async () => await _context.Products.FindAsync(request.Id))
                .TapIf(product => product is null, () => throw new NotFoundException(nameof(Product), request.Id))
                .Tap(product => _context.Products.Remove(product))
                .Tap(async () => await _context.SaveChangesAsync(cancellationToken));

            await result.Tap(product => _eventBus.PublishAsync(new ProductDeleted(product.ImageUri), cancellationToken));

            return result;
        }
    }
}
