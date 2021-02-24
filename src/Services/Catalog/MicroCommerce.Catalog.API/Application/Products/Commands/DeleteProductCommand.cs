using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;

namespace MicroCommerce.Catalog.API.Application.Products.Commands
{
    public class DeleteProductCommand : IRequest<Unit>
    {
        public int Id { get; init; }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public DeleteProductCommandHandler(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            await Result.Try(async () => await _context.Products.FindAsync(request.Id))
                .Tap(product => _context.Products.Remove(product))
                .Tap(async () => await _context.SaveChangesAsync(cancellationToken))
                .TapIf(product => File.Exists(product.ImageUri), product => File.Delete(product.ImageUri))
                .Tap(() => transaction.Complete());

            return Unit.Value;
        }
    }
}
