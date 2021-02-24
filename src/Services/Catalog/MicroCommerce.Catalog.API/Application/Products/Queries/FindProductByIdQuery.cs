using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.API.Application.Products.Queries
{
    public class FindProductByIdQuery : IRequest<ProductDto>
    {
        public int Id { get; init; }
    }

    public class FindProductByIdQueryHandler : IRequestHandler<FindProductByIdQuery, ProductDto>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public FindProductByIdQueryHandler(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<ProductDto> Handle(FindProductByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _context.Products.FindAsync(new object[] { request.Id }, cancellationToken);

            return _mapper.Map<ProductDto>(result);
        }
    }
}
