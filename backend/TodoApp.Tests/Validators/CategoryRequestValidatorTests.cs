using FluentAssertions;

using TodoApp.Api.DTOs.Categories;
using TodoApp.Api.Validators;

namespace TodoApp.Tests.Validators;

public sealed class CategoryRequestValidatorTests
{
    private readonly CreateCategoryRequestValidator _createValidator = new();
    private readonly UpdateCategoryRequestValidator _updateValidator = new();

    [Theory]
    [InlineData("Work")]
    [InlineData("  Trimmed to fifty  ")]
    public void ValidName_Passes(string name)
    {
        _createValidator.Validate(new CreateCategoryRequest(name)).IsValid.Should().BeTrue();
        _updateValidator.Validate(new UpdateCategoryRequest(name)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void FiftyCharactersPlusSpaces_Passes()
    {
        var name = $"  {new string('a', 50)}  ";

        _createValidator.Validate(new CreateCategoryRequest(name)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyName_Fails(string? name)
    {
        _createValidator.Validate(new CreateCategoryRequest(name!)).IsValid.Should().BeFalse();
        _updateValidator.Validate(new UpdateCategoryRequest(name!)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void NameLongerThan50_Fails()
    {
        var name = new string('a', 51);

        _createValidator.Validate(new CreateCategoryRequest(name)).IsValid.Should().BeFalse();
        _updateValidator.Validate(new UpdateCategoryRequest(name)).IsValid.Should().BeFalse();
    }
}
