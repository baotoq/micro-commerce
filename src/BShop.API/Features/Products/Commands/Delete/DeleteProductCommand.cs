using MediatR;

namespace BShop.API.Features.Products.Commands.Delete
{
    public class DeleteProductCommand : IRequest<bool>
    {
        public long Id { get; set; }

        public DeleteProductCommand(long id)
        {
            Id = id;
        }
    }
}
