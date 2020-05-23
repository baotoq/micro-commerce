using System.Threading;
using System.Threading.Tasks;
using BShop.API.Application.Categories.Models;
using BShop.API.Data;
using BShop.API.Data.Models;
using MediatR;

namespace BShop.API.Application.Categories.Commands.Put
{
    public class PutCategoryCommandHandler : IRequestHandler<PutCategoryCommand, CategoryDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Category> _repository;

        public PutCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.Repository<Category>();
        }

        public async Task<CategoryDto> Handle(PutCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.FindAsync(cancellationToken, request.Id);

            if (category == null)
            {
                return null;
            }

            category.Name = request.Name;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
