using Microsoft.AspNetCore.Identity;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Validators;

/// <summary>
/// Identity password rule: at least one letter (NFR-8). Identity itself checks length and digit.
/// </summary>
public sealed class LetterRequiredPasswordValidator : IPasswordValidator<AppUser>
{
    public const string ErrorCode = "PasswordRequiresLetter";

    public const string ErrorDescription = "Passwords must have at least one letter.";

    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
    {
        var result = password is not null && password.Any(char.IsLetter)
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Code = ErrorCode, Description = ErrorDescription });

        return Task.FromResult(result);
    }
}
