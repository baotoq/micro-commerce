using System.Threading;
using System.Threading.Tasks;
using Data.UnitOfWork.EF;
using Identity.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Exceptions;

namespace Identity.API.Application.Users.Commands
{
    public class UpdateUserRoleCommand : IRequest<Unit>
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }

        public UpdateUserRoleCommand(string userId, string roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }

    public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Unit>
    {
        private readonly IEfUnitOfWork _unitOfWork;
        private readonly IRepository<User, string> _userRepository;

        public UpdateUserRoleCommandHandler(IEfUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Repository<User, string>();
        }

        public async Task<Unit> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.Query()
                .Include(s => s.Roles)
                .SingleOrDefaultAsync(s => s.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException(nameof(User), request.UserId);
            }

            user.Roles.Clear();

            user.Roles.Add(new UserRole
            {
                RoleId = request.RoleId
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
