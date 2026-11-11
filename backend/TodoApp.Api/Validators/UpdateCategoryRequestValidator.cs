using FluentValidation;

using TodoApp.Api.DTOs.Categories;

namespace TodoApp.Api.Validators;

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(r => r.Name).ValidCategoryName();
    }
}
