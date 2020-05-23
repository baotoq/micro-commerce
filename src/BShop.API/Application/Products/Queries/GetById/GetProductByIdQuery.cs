using BShop.API.Application.Products.Models;
using MediatR;

namespace BShop.API.Application.Products.Queries.GetById
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
