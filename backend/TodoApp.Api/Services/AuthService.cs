using Microsoft.AspNetCore.Identity;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Auth;

namespace TodoApp.Api.Services;

public sealed class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AppDbContext dbContext,
    ITokenService tokenService) : IAuthService
{
    public const string EmailField = "email";

    public const string PasswordField = "password";

    public const string DuplicateEmailMessage = "An account with this email already exists.";

    public static readonly IReadOnlyList<string> DefaultCategoryNames = ["Work", "Personal"];

    private static readonly IdentityErrorDescriber ErrorCodes = new();

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var user = new AppUser
        {
            Name = request.Name,
            Email = request.Email,
            UserName = request.Email,
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var identityResult = await userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            return AuthResult.ValidationFailed(MapIdentityErrors(identityResult.Errors));
        }

        dbContext.Categories.AddRange(
            DefaultCategoryNames.Select(name => new Category { UserId = user.Id, Name = name }));
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return AuthResult.Success(CreateResponse(user));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult.InvalidCredentials();
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return AuthResult.LockedOut();
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (signInResult.Succeeded)
        {
            return AuthResult.Success(CreateResponse(user));
        }

        return signInResult.IsLockedOut ? AuthResult.LockedOut() : AuthResult.InvalidCredentials();
    }

    private AuthResponse CreateResponse(AppUser user)
    {
        var accessToken = tokenService.CreateToken(user);
        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            new AuthUserDto(user.Id, user.Name, user.Email ?? string.Empty));
    }

    private static Dictionary<string, string[]> MapIdentityErrors(IEnumerable<IdentityError> errors) =>
        errors
            .Select(error => (Field: GetField(error.Code), Message: GetMessage(error)))
            .GroupBy(error => error.Field)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Message).Distinct().ToArray());

    private static string GetField(string code) =>
        code.StartsWith("Password", StringComparison.Ordinal) ? PasswordField : EmailField;

    private static string GetMessage(IdentityError error) =>
        error.Code == ErrorCodes.DuplicateEmail(string.Empty).Code || error.Code == ErrorCodes.DuplicateUserName(string.Empty).Code
            ? DuplicateEmailMessage
            : error.Description;
}
