using System.Collections.Generic;
using BShop.API.Features.Products.Models;
using MediatR;

namespace BShop.API.Features.Products.Commands.Create
{
    public class CreateProductCommand : IRequest<ProductDto>
    {
        public string Name { get; set; }

        public IList<long> CategoryIds { get; set; }
    }
}
