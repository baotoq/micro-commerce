using System.Threading;
using System.Threading.Tasks;
using Data.UnitOfWork.EF;
using Identity.API.Data.Models;
using MediatR;
using Shared.MediatR.Exceptions;

namespace Identity.API.Application.Users.Commands
{
    public class DeleteUserCommand : IRequest<Unit>
    {
        public string Id { get; set; }

        public DeleteUserCommand(string id)
        {
            Id = id;
        }
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly IEfUnitOfWork _unitOfWork;
        private readonly IRepository<User, string> _userRepository;

        public DeleteUserCommandHandler(IEfUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User, string>();
        }

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindAsync(request.Id, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException(nameof(User), request.Id);
            }

            _userRepository.Remove(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
