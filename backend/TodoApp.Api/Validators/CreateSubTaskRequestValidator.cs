using FluentValidation;

using TodoApp.Api.DTOs.SubTasks;

namespace TodoApp.Api.Validators;

public sealed class CreateSubTaskRequestValidator : AbstractValidator<CreateSubTaskRequest>
{
    public CreateSubTaskRequestValidator()
    {
        RuleFor(r => r.Title).ValidSubTaskTitle();
    }
}
