using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Products.Models;
using Catalog.API.Data.Models;
using MediatR;
using Shared.MediatR.Models;
using UnitOfWork;
using UnitOfWork.Common;

namespace Catalog.API.Application.Products.Queries
{
    public class FindProductsQuery : OffsetPagedQuery, IRequest<OffsetPaged<ProductDto>>
    {
        public string QueryString { get; set; }
    }

    public class FindProductsQueryHandler : IRequestHandler<FindProductsQuery, OffsetPaged<ProductDto>>
    {
        private readonly IRepository<Product> _repository;

        public FindProductsQueryHandler(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<OffsetPaged<ProductDto>> Handle(FindProductsQuery request, CancellationToken cancellationToken)
        {
            var filterQuery = _repository.Query();

            if (!string.IsNullOrEmpty(request.QueryString))
            {
                request.QueryString = request.QueryString.ToLowerInvariant();
                filterQuery = filterQuery.Where(
                    s => s.Name.ToLower().Contains(request.QueryString) ||
                         s.Description.ToLower().Contains(request.QueryString));
            }

            var paged = await filterQuery.Select(s => new ProductDto
            {
                Id = s.Id,
                Name = s.Name,
                CartMaxQuantity = s.CartMaxQuantity,
                StockQuantity = s.StockQuantity,
                Price = s.Price,
                ImageUri = s.ImageUri,
                ReviewsCount = s.ReviewsCount,
                RatingAverage = s.RatingAverage,
                Description = s.Description
            }).ToPagedAsync(request.Page, request.PageSize, cancellationToken);

            return paged;
        }
    }
}
