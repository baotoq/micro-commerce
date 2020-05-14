using System.Text.Json.Serialization;
using BShop.API.Features.Products.Models;
using MediatR;

namespace BShop.API.Features.Products.Commands.Put
{
    public class PutProductCommand : IRequest<ProductDto>
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
