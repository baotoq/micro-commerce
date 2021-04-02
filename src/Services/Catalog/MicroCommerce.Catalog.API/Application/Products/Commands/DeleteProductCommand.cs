using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using MicroCommerce.Shared.FileStorage;
using MicroCommerce.Shared.MediatR.Exceptions;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public class DeleteProductCommand : IRequest<Result>
    {
        public int Id { get; init; }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;

        public DeleteProductCommandHandler(IMapper mapper, ApplicationDbContext context, IStorageService storageService)
        {
            _mapper = mapper;
            _context = context;
            _storageService = storageService;
        }

        public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            return await Result.Try(async () => await _context.Products.FindAsync(request.Id))
                .TapIf(product => product is null, () => throw new NotFoundException(nameof(Product), request.Id))
                .Tap(product => _context.Products.Remove(product))
                .Tap(async () => await _context.SaveChangesAsync(cancellationToken))
                .Tap(product => _storageService.DeleteAsync(product.ImageUri, cancellationToken))
                .Tap(transaction.Complete);
        }
    }
}
