using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using TodoApp.Api.Data.Entities;
using TodoApp.Api.Extensions;

namespace TodoApp.Api.Services;

public sealed class TokenService(IOptions<JwtSettings> options) : ITokenService
{
    public AccessToken CreateToken(AppUser user)
    {
        var settings = options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            Expires = expiresAt,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id,
                [JwtRegisteredClaimNames.Email] = user.Email ?? string.Empty,
                [JwtRegisteredClaimNames.Name] = user.Name,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            },
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new AccessToken(token, expiresAt);
    }
}
