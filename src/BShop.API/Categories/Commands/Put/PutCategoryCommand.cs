using System.Text.Json.Serialization;
using BShop.API.Categories.Models;
using MediatR;

namespace BShop.API.Categories.Commands.Put
{
    public class PutCategoryCommand : IRequest<CategoryDto>
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
