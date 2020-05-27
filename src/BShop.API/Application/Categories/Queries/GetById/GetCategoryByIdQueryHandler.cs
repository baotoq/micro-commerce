using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Application.Categories.Models;
using BShop.API.Data;
using BShop.API.Data.Models;
using BShop.API.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BShop.API.Application.Categories.Queries.GetById
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
                .Select(s => new CategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Products = s.ProductCategories.Select(s => new ProductDto
                    {
                        Id = s.Product!.Id,
                        Name = s.Product.Name,
                        Price = s.Product.Price,
                        Description = s.Product.Description,
                        ImageFileName = s.Product.ImageFileName
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
