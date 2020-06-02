using System.Text.Json.Serialization;
using MediatR;

namespace Catalog.API.Application.Categories.Commands.Put
{
    public class PutCategoryCommand : IRequest
    {
        [JsonIgnore]
        public long Id { get; set; }

        public string? Name { get; set; }
    }
}
