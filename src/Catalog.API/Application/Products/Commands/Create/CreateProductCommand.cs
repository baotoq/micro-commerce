using System.Collections.Generic;
using Catalog.API.Application.Products.Models;
using MediatR;

namespace Catalog.API.Application.Products.Commands.Create
{
    public class CreateProductCommand : IRequest<ProductDto>
    {
        public string Name { get; set; }

        public IList<long> CategoryIds { get; set; } = new List<long>();
    }
}
