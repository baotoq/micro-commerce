using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Categories.Models;
using Catalog.API.Data.Models;
using Data.UnitOfWork.EF.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Exceptions;

namespace Catalog.API.Application.Categories.Queries
{
    public class FindCategoryByIdQuery : IRequest<CategoryDto>
    {
        public long Id { get; set; }

        public FindCategoryByIdQuery(long id)
        {
            Id = id;
        }
    }

    public class FindCategoryByIdQueryHandler : IRequestHandler<FindCategoryByIdQuery, CategoryDto>
    {
        private readonly IRepository<Category> _repository;

        public FindCategoryByIdQueryHandler(IRepository<Category> repository)
        {
            _repository = repository;
        }

        public async Task<CategoryDto> Handle(FindCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _repository.Query()
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Products = c.Products.Select(s => new ProductDto
                    {
                        Id = s.Product.Id,
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
