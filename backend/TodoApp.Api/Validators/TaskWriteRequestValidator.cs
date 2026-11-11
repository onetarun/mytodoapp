using FluentValidation;

using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.Validators;

/// <summary>
/// Rules shared by create and update (FR-6). The category check goes through the
/// ownership query filter, so another user's (or a deleted) category is rejected.
/// </summary>
public abstract class TaskWriteRequestValidator<T> : AbstractValidator<T>
    where T : ITaskWriteRequest
{
    public const string TitleRequiredMessage = "Title is required.";

    public const string CategoryNotFoundMessage = "Category not found.";

    protected TaskWriteRequestValidator(AppDbContext dbContext)
    {
        RuleFor(r => r.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage(TitleRequiredMessage)
            .MaximumLength(TodoTask.TitleMaxLength);

        RuleFor(r => r.Description)
            .MaximumLength(TodoTask.DescriptionMaxLength);

        RuleFor(r => r.Priority)
            .IsInEnum();

        RuleFor(r => r.CategoryId)
            .MustAsync((categoryId, cancellationToken) =>
                dbContext.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken))
            .When(r => r.CategoryId is not null)
            .WithMessage(CategoryNotFoundMessage);
    }
}
