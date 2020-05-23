using System.Text.Json.Serialization;
using BShop.API.Application.Categories.Models;
using MediatR;

namespace BShop.API.Application.Categories.Commands.Put
{
    public class PutCategoryCommand : IRequest<CategoryDto>
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string? Name { get; set; }
    }
}
