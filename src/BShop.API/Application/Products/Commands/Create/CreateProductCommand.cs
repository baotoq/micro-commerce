using System.Collections.Generic;
using BShop.API.Application.Products.Models;
using MediatR;

namespace BShop.API.Application.Products.Commands.Create
{
    public class CreateProductCommand : IRequest<ProductDto>
    {
        public string? Name { get; set; }

        public IList<long> CategoryIds { get; set; } = new List<long>();
    }
}
