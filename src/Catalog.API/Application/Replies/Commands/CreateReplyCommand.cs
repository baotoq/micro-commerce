using System.Threading;
using System.Threading.Tasks;
using Catalog.API.Data.Models;
using Catalog.API.Data.Models.Enums;
using Catalog.API.Services;
using Data.UnitOfWork;
using FluentValidation;
using MediatR;

namespace Catalog.API.Application.Replies.Commands
{
    public class CreateReplyCommand : IRequest<Unit>
    {
        public string Comment { get; set; }
        public long ReviewId { get; set; }
    }

    public class CreateReplyCommandValidator : AbstractValidator<CreateReplyCommand>
    {
        public CreateReplyCommandValidator()
        {
            RuleFor(s => s.Comment).NotEmpty().MinimumLength(10);
        }
    }

    public class CreateReplyCommandHandler : IRequestHandler<CreateReplyCommand, Unit>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Reply> _repository;

        public CreateReplyCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork, IRepository<Reply> repository)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<Unit> Handle(CreateReplyCommand request, CancellationToken cancellationToken)
        {
            await _repository.AddAsync(new Reply
            {
                Comment = request.Comment,
                ReviewId = request.ReviewId,
                ReplyStatus = ReplyStatus.Pending,
                CreatedById = _identityService.GetCurrentUserId()
            }, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
