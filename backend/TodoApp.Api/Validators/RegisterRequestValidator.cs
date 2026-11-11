using FluentValidation;

using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Auth;
using TodoApp.Api.Extensions;

namespace TodoApp.Api.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public const string PasswordLetterMessage = "Password must contain at least one letter.";

    public const string PasswordDigitMessage = "Password must contain at least one number.";

    public RegisterRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(AppUser.NameMaxLength);

        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(IdentityPolicy.EmailMaxLength);

        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(IdentityPolicy.PasswordMinLength)
            .Must(p => p is not null && p.Any(char.IsLetter)).WithMessage(PasswordLetterMessage)
            .Must(p => p is not null && p.Any(char.IsAsciiDigit)).WithMessage(PasswordDigitMessage);
    }
}
