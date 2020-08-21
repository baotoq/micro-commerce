using System.Threading;
using System.Threading.Tasks;
using Data.UnitOfWork.EF.Common;
using Identity.API.Application.Users.Commands;
using Identity.API.Application.Users.Models;
using Identity.API.Application.Users.Queries;
using IdentityServer4;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.ApiControllers
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<OffsetPaged<UserDto>>> FindUsers([FromQuery] FindUsersQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);

            return result;
        }

        [HttpPut("{userId}/role/{roleId}")]
        public async Task<IActionResult> UpdateUserRole(string userId, string roleId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new UpdateUserRoleCommand(userId, roleId), cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteUserCommand(id), cancellationToken);

            return NoContent();
        }
    }
}
