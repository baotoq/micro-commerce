using System.Text.Json.Serialization;
using MediatR;

namespace BShop.API.Application.Categories.Commands.Put
{
    public class PutCategoryCommand : IRequest
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string? Name { get; set; }
    }
}
