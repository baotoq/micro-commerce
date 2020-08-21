using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Services;
using Data.UnitOfWork.EF;
using FluentValidation;
using MediatR;

namespace Catalog.API.Application.Reviews.Commands
{
    public class CreateReviewCommand : IRequest<Unit>
    {
        public string Title { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public long ProductId { get; set; }
    }

    public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewCommandValidator()
        {  
            RuleFor(s => s.Rating).InclusiveBetween(0, 5);
        }
    }

    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IEfUnitOfWork _unitOfWork;
        private readonly IRepository<Review> _repository;

        public CreateReviewCommandHandler(IIdentityService identityService, IEfUnitOfWork unitOfWork, IRepository<Review> repository)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<Unit> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            await _repository.AddAsync(new Review
            {
                Title = request.Title,
                Comment = request.Comment,
                Rating = request.Rating,
                ProductId = request.ProductId,
                CreatedById = _identityService.GetCurrentUserId()
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
