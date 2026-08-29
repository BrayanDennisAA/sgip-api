using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sgip.WebApi.Validations;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
    ActionExecutingContext context,
    ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType =
                typeof(IValidator<>)
                    .MakeGenericType(argument.GetType());

            if (context.HttpContext.RequestServices
                .GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext =
                new ValidationContext<object>(argument);

            var validationResult =
                await validator.ValidateAsync(validationContext);

            if (validationResult.IsValid)
                continue;

            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray());

            context.Result =
                new BadRequestObjectResult(
                    new ValidationProblemDetails(errors)
                    {
                        Title = "Validation Error",
                        Status = StatusCodes.Status400BadRequest
                    });

            return;
        }

        await next();
    }
}