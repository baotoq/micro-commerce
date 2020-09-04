using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Entities.Common;
using Data.UnitOfWork.EF.Common;
using Data.UnitOfWork.EF.Core;
using Identity.API.Application.Users.Models;
using Identity.API.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.MediatR.Models;

namespace Identity.API.Application.Users.Queries
{
    public class FindUsersQuery : OffsetPagedQuery, IRequest<OffsetPaged<UserDto>>
    {
        public string QueryString { get; set; }
    }

    public class FindUsersQueryHandler : IRequestHandler<FindUsersQuery, OffsetPaged<UserDto>>
    {
        private readonly IRepository<User, string> _repository;

        public FindUsersQueryHandler(IRepository<User, string> repository)
        {
            _repository = repository;
        }

        public async Task<OffsetPaged<UserDto>> Handle(FindUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.Query().Include(s => s.Roles).AsQueryable();

            if (!string.IsNullOrEmpty(request.QueryString))
            {
                request.QueryString = request.QueryString.ToUpperInvariant();
                query = query.Where(s => s.NormalizedEmail.Contains(request.QueryString));
            }

            var result = await query
                .Select(s => new UserDto
                {
                    Id = s.Id,
                    Email = s.Email,
                    UserName = s.UserName,
                    RoleId = s.Roles.FirstOrDefault().RoleId
                })
                .ToPagedAsync(request.Page, request.PageSize, cancellationToken);

            return result;
        }
    }
}
