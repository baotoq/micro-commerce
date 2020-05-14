using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BShop.API.Data;
using BShop.API.Data.Models;
using BShop.API.Features.Products.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BShop.API.Features.Products.Queries.GetAll
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
                .Select(x => new ProductDto { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);

            return result;
        }
    }
}
