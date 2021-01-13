using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MicroCommerce.Catalog.API.Application.Products.Models;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.API.Application.Products.Queries
{
    public class FindProductsQuery : OffsetPagedQuery, IRequest<OffsetPaged<ProductDto>>
    {
    }

    public class FindProductsQueryHandler : IRequestHandler<FindProductsQuery, OffsetPaged<ProductDto>>
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public FindProductsQueryHandler(IMapper mapper, ApplicationDbContext context)
        {
            _mapper = mapper;
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<OffsetPaged<ProductDto>> Handle(FindProductsQuery request, CancellationToken cancellationToken)
        {
            var result = await _context.Products.ToPagedAsync(request.Page, request.PageSize, cancellationToken);

            return _mapper.Map<OffsetPaged<ProductDto>>(result);
        }
    }
}
