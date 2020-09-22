using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Catalog.API.Services
{
    public interface IIdentityService
    {
        string GetCurrentUserId();
    }

    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _accessor;

        public IdentityService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string GetCurrentUserId()
        {
            return _accessor.HttpContext.User.FindFirstValue("sub");
        }
    }
}
