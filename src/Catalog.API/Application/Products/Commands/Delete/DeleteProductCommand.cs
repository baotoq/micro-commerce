using MediatR;

namespace Catalog.API.Application.Products.Commands.Delete
{
    public class DeleteProductCommand : IRequest
    {
        public long Id { get; set; }

        public DeleteProductCommand(long id)
        {
            Id = id;
        }
    }
}
