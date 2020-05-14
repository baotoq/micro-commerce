using System.Text.Json.Serialization;
using BShop.API.Features.Categories.Models;
using MediatR;

namespace BShop.API.Features.Categories.Commands.Put
{
    public class PutCategoryCommand : IRequest<CategoryDto>
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
