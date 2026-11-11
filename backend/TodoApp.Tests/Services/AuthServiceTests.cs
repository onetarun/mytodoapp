using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

using TodoApp.Api.Data;
using TodoApp.Api.DTOs.Auth;
using TodoApp.Api.Extensions;
using TodoApp.Api.Services;

namespace TodoApp.Tests.Services;

public sealed class AuthServiceTests : IAsyncLifetime
{
    private const string Name = "Asha Verma";
    private const string Email = "asha@example.test";
    private const string Password = "testpass1";
    private const string WrongPassword = "wrongpass1";

    private readonly AuthTestHost _host = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Register_CreatesUserWithHashedPassword_AndDefaultCategories()
    {
        var result = await RegisterAsync(Email, Password);

        result.Status.Should().Be(AuthResultStatus.Success);
        result.Response!.User.Email.Should().Be(Email);
        result.Response.User.Name.Should().Be(Name);
        result.Response.Token.Should().NotBeNullOrWhiteSpace();

        await using var scope = _host.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync();
        user.Id.Should().Be(result.Response.User.Id);
        user.PasswordHash.Should().NotBeNullOrWhiteSpace().And.NotBe(Password);
        user.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);

        var categoryNames = await db.Categories.IgnoreQueryFilters()
            .Where(c => c.UserId == user.Id)
            .Select(c => c.Name)
            .ToListAsync();
        categoryNames.Should().BeEquivalentTo(AuthService.DefaultCategoryNames);
    }

    [Fact]
    public async Task Register_DuplicateEmail_FailsWithEmailError_AndCreatesNothing()
    {
        await RegisterAsync(Email, Password);

        var result = await RegisterAsync(Email.ToUpperInvariant(), Password);

        result.Status.Should().Be(AuthResultStatus.ValidationFailed);
        result.Response.Should().BeNull();
        result.Errors.Should().ContainKey(AuthService.EmailField);
        result.Errors[AuthService.EmailField].Should().ContainSingle().Which.Should().Be(AuthService.DuplicateEmailMessage);

        await using var scope = _host.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Users.CountAsync()).Should().Be(1);
        (await db.Categories.IgnoreQueryFilters().CountAsync()).Should().Be(AuthService.DefaultCategoryNames.Count);
    }

    [Theory]
    [InlineData("abcdefgh")]
    [InlineData("12345678")]
    [InlineData("abc123")]
    public async Task Register_WeakPassword_FailsWithPasswordError_AndCreatesNothing(string weakPassword)
    {
        var result = await RegisterAsync(Email, weakPassword);

        result.Status.Should().Be(AuthResultStatus.ValidationFailed);
        result.Errors.Should().ContainKey(AuthService.PasswordField);

        await using var scope = _host.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Users.AnyAsync()).Should().BeFalse();
        (await db.Categories.IgnoreQueryFilters().AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Login_CorrectCredentials_ReturnsTokenWithClaims_And60MinuteExpiry()
    {
        var registered = await RegisterAsync(Email, Password);
        var before = DateTime.UtcNow;

        var result = await LoginAsync(Email, Password);

        result.Status.Should().Be(AuthResultStatus.Success);
        var expectedExpiry = before.AddMinutes(AuthTestHost.ExpiryMinutes);
        result.Response!.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));

        var token = new JsonWebTokenHandler().ReadJsonWebToken(result.Response.Token);
        token.Subject.Should().Be(registered.Response!.User.Id);
        token.GetClaim(JwtRegisteredClaimNames.Email).Value.Should().Be(Email);
        token.GetClaim(JwtRegisteredClaimNames.Name).Value.Should().Be(Name);
        token.Id.Should().NotBeNullOrWhiteSpace();
        token.Issuer.Should().Be(AuthTestHost.Issuer);
        token.Audiences.Should().ContainSingle().Which.Should().Be(AuthTestHost.Audience);
        token.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsInvalidCredentials()
    {
        await RegisterAsync(Email, Password);

        var result = await LoginAsync(Email, WrongPassword);

        result.Status.Should().Be(AuthResultStatus.InvalidCredentials);
        result.Response.Should().BeNull();
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsInvalidCredentials()
    {
        var result = await LoginAsync("nobody@example.test", Password);

        result.Status.Should().Be(AuthResultStatus.InvalidCredentials);
    }

    [Fact]
    public async Task Login_FiveWrongPasswords_LockAccount_AndSixthWithCorrectPasswordIsLocked()
    {
        await RegisterAsync(Email, Password);

        for (var attempt = 1; attempt < IdentityPolicy.MaxFailedAccessAttempts; attempt++)
        {
            (await LoginAsync(Email, WrongPassword)).Status.Should().Be(AuthResultStatus.InvalidCredentials);
        }

        (await LoginAsync(Email, WrongPassword)).Status.Should().Be(AuthResultStatus.LockedOut);
        (await LoginAsync(Email, Password)).Status.Should().Be(AuthResultStatus.LockedOut);
    }

    [Fact]
    public async Task Login_SuccessResetsFailedAttemptCount()
    {
        await RegisterAsync(Email, Password);

        for (var attempt = 1; attempt < IdentityPolicy.MaxFailedAccessAttempts; attempt++)
        {
            await LoginAsync(Email, WrongPassword);
        }

        (await LoginAsync(Email, Password)).Status.Should().Be(AuthResultStatus.Success);
        (await LoginAsync(Email, WrongPassword)).Status.Should().Be(AuthResultStatus.InvalidCredentials);
    }

    private async Task<AuthResult> RegisterAsync(string email, string password)
    {
        await using var scope = _host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAuthService>();
        return await service.RegisterAsync(new RegisterRequest(Name, email, password));
    }

    private async Task<AuthResult> LoginAsync(string email, string password)
    {
        await using var scope = _host.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAuthService>();
        return await service.LoginAsync(new LoginRequest(email, password));
    }
}
