using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Services;

public interface ITokenService
{
    /// <summary>
    /// Creates an HMAC-SHA256 JWT with the <c>sub</c>, <c>email</c>, <c>name</c> and <c>jti</c> claims (AD-4).
    /// </summary>
    AccessToken CreateToken(AppUser user);
}
