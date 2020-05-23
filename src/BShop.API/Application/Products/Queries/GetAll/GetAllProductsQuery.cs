using System.Collections.Generic;
using BShop.API.Application.Products.Models;
using MediatR;

namespace BShop.API.Application.Products.Queries.GetAll
{
    public class GetAllProductsQuery : IRequest<List<ProductDto>>
    {
    }
}
