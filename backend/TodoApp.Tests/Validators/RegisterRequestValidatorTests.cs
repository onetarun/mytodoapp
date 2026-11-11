using FluentAssertions;

using TodoApp.Api.DTOs.Auth;
using TodoApp.Api.Validators;

namespace TodoApp.Tests.Validators;

public sealed class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_Passes()
    {
        var result = _validator.Validate(new RegisterRequest("Asha", "asha@example.test", "testpass1"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("abcdefgh")]
    [InlineData("12345678")]
    [InlineData("abc123")]
    [InlineData("")]
    public void WeakPassword_Fails(string password)
    {
        var result = _validator.Validate(new RegisterRequest("Asha", "asha@example.test", password));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().OnlyContain(e => e.PropertyName == nameof(RegisterRequest.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void InvalidEmail_Fails(string email)
    {
        var result = _validator.Validate(new RegisterRequest("Asha", email, "testpass1"));

        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Fact]
    public void MissingOrTooLongName_Fails()
    {
        _validator.Validate(new RegisterRequest("", "asha@example.test", "testpass1"))
            .Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Name));
        _validator.Validate(new RegisterRequest(new string('a', 101), "asha@example.test", "testpass1"))
            .Errors.Should().Contain(e => e.PropertyName == nameof(RegisterRequest.Name));
    }
}
