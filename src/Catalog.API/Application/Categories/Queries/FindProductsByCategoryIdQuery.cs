using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Exceptions;
using UnitOfWork;
using UnitOfWork.Common;

namespace Catalog.API.Application.Categories.Queries
{
    public class FindProductsByCategoryIdQuery : IRequest<OffsetPaged<ProductDto>>
    {
        public long Id { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }

    public class FindProductsByCategoryIdQueryHandler : IRequestHandler<FindProductsByCategoryIdQuery, OffsetPaged<ProductDto>>
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Product> _productRepository;

        public FindProductsByCategoryIdQueryHandler(IRepository<Category> categoryRepository, IRepository<Product> productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<OffsetPaged<ProductDto>> Handle(FindProductsByCategoryIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.FindAsync(request.Id, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException(nameof(Category), request.Id);
            }

            var products = await _productRepository.Query()
                .Where(s => s.Categories.Any(c => c.CategoryId == category.Id))
                .Select(s => new ProductDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    Description = s.Description,
                    ImageUri = s.ImageUri
                })
                .AsNoTracking()
                .ToPagedAsync(request.Page, request.PageSize, cancellationToken);

            return products;
        }
    }
}
