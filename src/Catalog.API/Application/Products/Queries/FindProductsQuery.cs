using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Products.Models;
using Catalog.API.Common;
using Catalog.API.Data.Models;
using MediatR;
using UnitOfWork;

namespace Catalog.API.Application.Products.Queries
{
    public class FindProductsQuery : IRequest<List<ProductDto>>
    {
        public int Page { get; set; }

        public int PageSize { get; set; }
    }

    public class FindProductsQueryHandler : IRequestHandler<FindProductsQuery, List<ProductDto>>
    {
        private readonly IRepository<Product> _repository;

        public FindProductsQueryHandler(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductDto>> Handle(FindProductsQuery request, CancellationToken cancellationToken)
        {
            var paged = await _repository.Query().Select(s => new ProductDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                ImageUri = s.ImageUri,
                Description = s.Description
            }).ToPagedAsync(request.Page, request.PageSize, cancellationToken);

            return paged;
        }
    }
}
