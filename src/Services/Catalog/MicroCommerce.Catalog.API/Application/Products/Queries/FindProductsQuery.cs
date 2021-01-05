using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MicroCommerce.Catalog.API.Infrastructure;
using MicroCommerce.Catalog.API.Persistence;
using MicroCommerce.Catalog.API.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.API.Application.Products.Queries
{
    public class FindProductsQuery : OffsetPagedQuery, IRequest<OffsetPaged<Product>>
    {
    }

    public class FindProductsQueryHandler : IRequestHandler<FindProductsQuery, OffsetPaged<Product>>
    {
        private readonly ApplicationDbContext _context;

        public FindProductsQueryHandler(ApplicationDbContext context)
        {
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<OffsetPaged<Product>> Handle(FindProductsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Products.ToPagedAsync(request.Page, request.PageSize, cancellationToken);
        }
    }
}
