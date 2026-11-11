using FluentValidation;

using TodoApp.Api.DTOs.Auth;

namespace TodoApp.Api.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty();
        RuleFor(r => r.Password).NotEmpty();
    }
}
