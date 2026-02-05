using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MicroCommerce.ApiService.Features.Cart;

/// <summary>
/// Helper for resolving buyer identity from authenticated user claims or guest cookies.
/// Authenticated users use the "sub" claim; guests get a cookie-based GUID.
/// </summary>
public static class BuyerIdentity
{
    private const string CookieName = "buyer_id";

    public static Guid GetOrCreateBuyerId(HttpContext context)
    {
        // 1. Authenticated user: use sub claim
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub");

        if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var authenticatedId))
        {
            return authenticatedId;
        }

        // 2. Existing guest cookie
        if (context.Request.Cookies.TryGetValue(CookieName, out var cookieValue)
            && Guid.TryParse(cookieValue, out var guestId))
        {
            return guestId;
        }

        // 3. New guest: generate GUID and set cookie
        var newId = Guid.NewGuid();

        context.Response.Cookies.Append(CookieName, newId.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(7),
            Path = "/"
        });

        return newId;
    }
}
