using System.Security.Claims;

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using TodoApp.Api.Services;

namespace TodoApp.Tests.Services;

public sealed class CurrentUserServiceTests
{
    private const string UserId = "3f2c9a10-user";

    [Fact]
    public void UserId_ReturnsNameIdentifierClaim_WhenAuthenticated()
    {
        var service = CreateService(new Claim(ClaimTypes.NameIdentifier, UserId));

        service.UserId.Should().Be(UserId);
    }

    [Fact]
    public void UserId_ReturnsSubClaim_WhenClaimsAreNotMapped()
    {
        var service = CreateService(new Claim("sub", UserId));

        service.UserId.Should().Be(UserId);
    }

    [Fact]
    public void UserId_ReturnsNull_WhenNotAuthenticated()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, UserId)])),
        };
        var service = new CurrentUserService(new HttpContextAccessor { HttpContext = context });

        service.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_ReturnsNull_WhenThereIsNoHttpContext()
    {
        var service = new CurrentUserService(new HttpContextAccessor());

        service.UserId.Should().BeNull();
    }

    private static CurrentUserService CreateService(Claim claim)
    {
        var identity = new ClaimsIdentity([claim], authenticationType: "Bearer");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        return new CurrentUserService(new HttpContextAccessor { HttpContext = context });
    }
}
