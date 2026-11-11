using FluentAssertions;

using TodoApp.Api.Data.Entities;
using TodoApp.Api.Validators;

namespace TodoApp.Tests.Validators;

public sealed class LetterRequiredPasswordValidatorTests
{
    private readonly LetterRequiredPasswordValidator _validator = new();

    [Fact]
    public async Task PasswordWithLetter_Succeeds()
    {
        var result = await _validator.ValidateAsync(null!, new AppUser(), "1234567a");

        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData(null)]
    public async Task PasswordWithoutLetter_Fails(string? password)
    {
        var result = await _validator.ValidateAsync(null!, new AppUser(), password);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be(LetterRequiredPasswordValidator.ErrorCode);
    }
}
