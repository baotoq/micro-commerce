using Catalog.API.Application.Products.Models;
using MediatR;

namespace Catalog.API.Application.Products.Queries.GetById
{
    public class GetProductByIdQuery : IRequest<ProductDto>
    {
        public long Id { get; set; }

        public GetProductByIdQuery(long id)
        {
            Id = id;
        }
    }
}
