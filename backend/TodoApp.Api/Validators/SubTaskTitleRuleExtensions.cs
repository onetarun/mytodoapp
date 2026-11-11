using FluentValidation;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Validators;

public static class SubTaskTitleRuleExtensions
{
    public const string RequiredMessage = "Title is required.";

    /// <summary>
    /// Subtask title rules: required (not blank) and at most 200 characters.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidSubTaskTitle<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage(RequiredMessage)
            .MaximumLength(SubTask.TitleMaxLength);
}
