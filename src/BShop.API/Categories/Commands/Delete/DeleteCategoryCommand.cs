using MediatR;

namespace BShop.API.Categories.Commands.Delete
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeleteCategoryCommand(int id)
        {
            Id = id;
        }
    }
}
