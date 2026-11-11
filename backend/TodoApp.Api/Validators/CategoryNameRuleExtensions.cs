using FluentValidation;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Validators;

public static class CategoryNameRuleExtensions
{
    public const string RequiredMessage = "Name is required.";

    /// <summary>
    /// Category name rules: required and at most 50 characters after trimming.
    /// Uniqueness per user needs the database and is checked in <c>CategoryService</c>.
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidCategoryName<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage(RequiredMessage)
            .Must(name => name is null || name.Trim().Length <= Category.NameMaxLength)
            .WithMessage($"Name must be {Category.NameMaxLength} characters or fewer.");
}
