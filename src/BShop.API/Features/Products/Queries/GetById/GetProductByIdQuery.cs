using BShop.API.Features.Products.Models;
using MediatR;

namespace BShop.API.Features.Products.Queries.GetById
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
