using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MicroCommerce.Catalog.API.Persistence;

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
            var product = await _context.Products.FindAsync(request.Id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
