using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Application.Products.Models;
using Catalog.API.Data;
using Catalog.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Application.Products.Queries.GetAll
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
    {
        private readonly IRepository<Product> _repository;

        public GetAllProductsQueryHandler(IRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.Query()
                .Select(s => new ProductDto
                { 
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    ImageUri = s.ImageUri,
                    Description = s.Description
                })
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
