using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TodoApp.Api.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // JwtBearer maps "sub" to ClaimTypes.NameIdentifier by default; fall back to the raw "sub" claim.
            return user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }
}
