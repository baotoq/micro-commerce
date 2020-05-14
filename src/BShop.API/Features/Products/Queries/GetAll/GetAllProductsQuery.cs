using System.Collections.Generic;
using BShop.API.Features.Products.Models;
using MediatR;

namespace BShop.API.Features.Products.Queries.GetAll
{
    public class GetAllProductsQuery : IRequest<List<ProductDto>>
    {
    }
}
