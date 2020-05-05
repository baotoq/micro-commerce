using MediatR;

namespace BShop.API.Categories.Commands.Delete
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public string Id { get; set; }

        public DeleteCategoryCommand(int id)
        {
            Id = id;
        }
    }
}
