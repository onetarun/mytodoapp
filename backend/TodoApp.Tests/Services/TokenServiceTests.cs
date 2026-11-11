using System.Text;

using FluentAssertions;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using TodoApp.Api.Data.Entities;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Services;

public sealed class TokenServiceTests
{
    private static readonly AppUser User = new()
    {
        Id = "user-1",
        Name = "Ravi",
        Email = "ravi@example.test",
    };

    private readonly TokenService _service = new(AuthTestHost.JwtOptions);

    [Fact]
    public async Task CreateToken_IsValidForConfiguredIssuerAudienceAndKey()
    {
        var accessToken = _service.CreateToken(User);

        var result = await ValidateAsync(accessToken.Token, AuthTestHost.SigningKey);

        result.IsValid.Should().BeTrue();
        result.Claims[JwtRegisteredClaimNames.Sub].Should().Be(User.Id);
        result.Claims[JwtRegisteredClaimNames.Email].Should().Be(User.Email);
        result.Claims[JwtRegisteredClaimNames.Name].Should().Be(User.Name);
        accessToken.ExpiresAt.Kind.Should().Be(DateTimeKind.Utc);
        accessToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(AuthTestHost.ExpiryMinutes), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateToken_FailsValidationWithAnotherKey()
    {
        var accessToken = _service.CreateToken(User);

        var result = await ValidateAsync(accessToken.Token, "another-test-signing-key-0123456789-abcdefghij");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateToken_UsesNewJtiForEveryToken()
    {
        var handler = new JsonWebTokenHandler();

        var first = handler.ReadJsonWebToken(_service.CreateToken(User).Token);
        var second = handler.ReadJsonWebToken(_service.CreateToken(User).Token);

        first.Id.Should().NotBe(second.Id);
    }

    private static Task<TokenValidationResult> ValidateAsync(string token, string key) =>
        new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = AuthTestHost.Issuer,
            ValidAudience = AuthTestHost.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        });
}
