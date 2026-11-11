using FluentAssertions;

using TodoApp.Api.DTOs.Auth;
using TodoApp.Api.Validators;

namespace TodoApp.Tests.Validators;

public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void FilledRequest_Passes()
    {
        _validator.Validate(new LoginRequest("asha@example.test", "testpass1")).IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyEmailAndPassword_Fail()
    {
        var result = _validator.Validate(new LoginRequest("", ""));

        result.Errors.Select(e => e.PropertyName).Should()
            .BeEquivalentTo([nameof(LoginRequest.Email), nameof(LoginRequest.Password)]);
    }
}
