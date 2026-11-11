using FluentValidation;

using TodoApp.Api.DTOs.Categories;

namespace TodoApp.Api.Validators;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(r => r.Name).ValidCategoryName();
    }
}
