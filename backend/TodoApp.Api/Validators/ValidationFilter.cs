using System.Text.Json;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace TodoApp.Api.Validators;

/// <summary>
/// Runs the FluentValidation validator of every action argument that has one and returns
/// a 400 ValidationProblemDetails with camelCase field names when validation fails.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            foreach (var failure in result.Errors)
            {
                context.ModelState.AddModelError(
                    JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName),
                    failure.ErrorMessage);
            }
        }

        if (!context.ModelState.IsValid)
        {
            var problemDetails = services.GetRequiredService<ProblemDetailsFactory>()
                .CreateValidationProblemDetails(context.HttpContext, context.ModelState);
            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        await next();
    }
}
