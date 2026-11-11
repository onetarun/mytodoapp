using FluentValidation;

using TodoApp.Api.DTOs.SubTasks;

namespace TodoApp.Api.Validators;

public sealed class UpdateSubTaskRequestValidator : AbstractValidator<UpdateSubTaskRequest>
{
    public UpdateSubTaskRequestValidator()
    {
        RuleFor(r => r.Title).ValidSubTaskTitle();
    }
}
