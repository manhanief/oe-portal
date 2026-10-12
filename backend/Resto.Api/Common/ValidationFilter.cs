using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Resto.Api.Common;

/// <summary>
/// Runs the matching FluentValidation validator for every request body automatically,
/// before the controller action runs. If validation fails, the action is skipped and
/// a 400 response lists the problems (like Laravel Form Requests).
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument),
                context.HttpContext.RequestAborted);

            foreach (var error in result.Errors)
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!context.ModelState.IsValid)
        {
            var factory = services.GetRequiredService<ProblemDetailsFactory>();
            var problem = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
            context.Result = new BadRequestObjectResult(problem);
            return;
        }

        await next();
    }
}
