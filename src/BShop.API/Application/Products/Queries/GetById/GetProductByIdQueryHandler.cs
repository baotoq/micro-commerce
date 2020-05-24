using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Application.Products.Models;
using BShop.API.Data;
using BShop.API.Data.Models;
using BShop.API.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BShop.API.Application.Products.Queries.GetById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        private readonly IRepository<Product> _repository;

        public GetProductByIdQueryHandler(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _repository.Query()
                .Include(s => s.ProductCategories)
                .ThenInclude(s => s.Category)
                .SingleOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException(nameof(Product), request.Id);
            }

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Categories = product.ProductCategories.Select(s => new ProductCategoryDto
                {
                    Id = s.CategoryId,
                    Name = s.Category!.Name
                }).ToList()
            };
        }
    }
}
