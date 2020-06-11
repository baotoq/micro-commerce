using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Exceptions;
using UnitOfWork;

namespace Catalog.API.Application.Categories.Queries.GetById
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
    {
        private readonly IRepository<Category> _repository;

        public GetCategoryByIdQueryHandler(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _repository.Query()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Products = c.ProductCategories.Select(s => new ProductDto
                    {
                        Id = s.Product!.Id,
                        Name = s.Product.Name,
                        Price = s.Product.Price,
                        Description = s.Product.Description,
                        ImageUri = s.Product.ImageUri
                    }).ToList()
                })
                .SingleOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException(nameof(Category), request.Id);
            }

            return category;
        }
    }
}
