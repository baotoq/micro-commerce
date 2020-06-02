using System.Collections.Generic;
using Catalog.API.Application.Products.Models;
using MediatR;

namespace Catalog.API.Application.Products.Queries.GetAll
{
    public class GetAllProductsQuery : IRequest<List<ProductDto>>
    {
    }
}
