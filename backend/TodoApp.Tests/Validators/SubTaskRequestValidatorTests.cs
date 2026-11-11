using FluentAssertions;

using TodoApp.Api.DTOs.SubTasks;
using TodoApp.Api.Validators;

namespace TodoApp.Tests.Validators;

public sealed class SubTaskRequestValidatorTests
{
    private readonly CreateSubTaskRequestValidator _createValidator = new();
    private readonly UpdateSubTaskRequestValidator _updateValidator = new();

    [Fact]
    public void TitleUpTo200_Passes()
    {
        _createValidator.Validate(new CreateSubTaskRequest(new string('a', 200))).IsValid.Should().BeTrue();
        _updateValidator.Validate(new UpdateSubTaskRequest("Pack")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyTitle_Fails(string? title)
    {
        _createValidator.Validate(new CreateSubTaskRequest(title!)).IsValid.Should().BeFalse();
        _updateValidator.Validate(new UpdateSubTaskRequest(title!)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void TitleOver200_Fails()
    {
        var title = new string('a', 201);

        _createValidator.Validate(new CreateSubTaskRequest(title)).IsValid.Should().BeFalse();
        _updateValidator.Validate(new UpdateSubTaskRequest(title)).IsValid.Should().BeFalse();
    }
}
