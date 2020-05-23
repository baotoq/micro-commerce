using MediatR;

namespace BShop.API.Application.Categories.Commands.Delete
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public long Id { get; set; }

        public DeleteCategoryCommand(long id)
        {
            Id = id;
        }
    }
}
